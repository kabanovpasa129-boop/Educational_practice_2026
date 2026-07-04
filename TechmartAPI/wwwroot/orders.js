const API_URL = window.location.origin;
let currentUser = null;
let cart = JSON.parse(localStorage.getItem('cart')) || [];

const authBtn = document.getElementById('authBtn');
const userMenu = document.getElementById('userMenu');
const logoutBtn = document.getElementById('logoutBtn');
const loginForm = document.getElementById('loginForm');
const modal = document.getElementById('authModal');
const cartBtn = document.getElementById('cartBtn');
const cartModal = document.getElementById('cartModal');
const cartModalClose = document.getElementById('cartModalClose');
const cartItemsList = document.getElementById('cartItemsList');
const cartTotalSpan = document.getElementById('cartTotal');
const clearCartBtn = document.getElementById('clearCartBtn');
const checkoutBtn = document.getElementById('checkoutBtn');
const ordersList = document.getElementById('ordersList');

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('ru-RU') + ' ' + date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
}

function getStatusClass(status) {
    switch(status) {
        case 'новый': return 'status-new';
        case 'обработан': return 'status-processed';
        case 'доставлен': return 'status-delivered';
        case 'отменён': return 'status-cancelled';
        default: return '';
    }
}

function getStatusText(status) {
    switch(status) {
        case 'новый': return 'Новый';
        case 'обработан': return 'Обработан';
        case 'доставлен': return 'Доставлен';
        case 'отменён': return 'Отменён';
        default: return status;
    }
}

async function loadOrders() {
    if (!currentUser) {
        ordersList.innerHTML = '<div class="empty-orders"><h3>Необходимо войти в аккаунт</h3><button class="btn btn--primary" id="loginRedirectBtn">Войти</button></div>';
        const loginBtn = document.getElementById('loginRedirectBtn');
        if (loginBtn) {
            loginBtn.addEventListener('click', () => modal.style.display = 'flex');
        }
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/Orders/user/${currentUser.id}`);
        const orders = await response.json();

        if (orders.length === 0) {
            ordersList.innerHTML = '<div class="empty-orders"><h3>У вас пока нет заказов</h3><p>Перейдите в каталог и сделайте первый заказ</p><a href="catalog.html" class="btn btn--primary">Перейти в каталог</a></div>';
            return;
        }

        ordersList.innerHTML = orders.map(order => {
            const itemsHtml = order.items.map(item => `
                <div class="order-item">
                    <div>
                        <strong>${item.name}</strong><br>
                        <small>${item.category}</small>
                    </div>
                    <div>
                        ${item.quantity} шт. × ${Number(item.price).toLocaleString()} ₽ = ${Number(item.total).toLocaleString()} ₽
                    </div>
                </div>
            `).join('');

            const total = order.items.reduce((sum, item) => sum + item.total, 0);

            return `
                <div class="order-card">
                    <div class="order-header">
                        <div class="order-number">Заказ №${order.id}</div>
                        <div class="order-date">${formatDate(order.orderDate)}</div>
                        <div class="order-status ${getStatusClass(order.status)}">${getStatusText(order.status)}</div>
                    </div>
                    <div class="order-items">
                        ${itemsHtml}
                    </div>
                    <div class="order-total">
                        Итого: ${total.toLocaleString()} ₽
                    </div>
                </div>
            `;
        }).join('');
    } catch (error) {
        console.error('Ошибка загрузки заказов:', error);
        ordersList.innerHTML = '<div class="empty-orders"><h3>Ошибка загрузки заказов</h3></div>';
    }
}

async function getAllProducts() {
    const response = await fetch(`${API_URL}/api/Products`);
    return response.json();
}

async function addToCartById(productId) {
    try {
        const allProducts = await getAllProducts();
        const product = allProducts.find(p => p.id === productId);
        if (product) {
            cart.push(product);
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartCount();
            showNotification('Товар добавлен в корзину');
        }
    } catch (error) {
        console.error('Ошибка:', error);
    }
}

function updateCartCount() {
    const countEl = document.getElementById('cartCount');
    if (countEl) countEl.textContent = cart.length;
}

function showNotification(msg) {
    let notif = document.querySelector('.notification-toast');
    if (!notif) {
        notif = document.createElement('div');
        notif.className = 'notification-toast';
        notif.style.cssText = 'position:fixed; bottom:20px; right:20px; background:#2C2C2C; color:white; padding:12px 20px; border-radius:40px; z-index:1000; opacity:0; transition:0.2s;';
        document.body.appendChild(notif);
    }
    notif.textContent = msg;
    notif.style.opacity = '1';
    setTimeout(() => notif.style.opacity = '0', 2000);
}

function renderCart() {
    if (!cartItemsList) return;
    if (cart.length === 0) {
        cartItemsList.innerHTML = '<div class="loading">Корзина пуста</div>';
        if (cartTotalSpan) cartTotalSpan.innerHTML = '';
        return;
    }
    let total = 0;
    cartItemsList.innerHTML = cart.map((p, index) => {
        total += Number(p.price);
        return `<div style="display:flex; justify-content:space-between; padding:12px; border-bottom:1px solid #eee;">
            <div><strong>${p.name}</strong><br><small>${p.category}</small></div>
            <div>${Number(p.price).toLocaleString()} ₽ <button class="remove-from-cart" data-index="${index}" style="background:none; border:none; color:#D96E6E; cursor:pointer;">&times;</button></div>
        </div>`;
    }).join('');
    if (cartTotalSpan) cartTotalSpan.innerHTML = `Итого: ${total.toLocaleString()} ₽`;
    document.querySelectorAll('.remove-from-cart').forEach(btn => {
        btn.addEventListener('click', () => removeFromCart(parseInt(btn.dataset.index)));
    });
}

function removeFromCart(index) {
    cart.splice(index, 1);
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
    renderCart();
}

function clearCart() {
    cart = [];
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
    renderCart();
    showNotification('Корзина очищена');
}

async function checkout() {
    if (cart.length === 0) {
        showNotification('Корзина пуста');
        return;
    }
    if (!currentUser) {
        showNotification('Необходимо войти в аккаунт');
        modal.style.display = 'flex';
        return;
    }
    try {
        const response = await fetch(`${API_URL}/api/Orders/create`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: currentUser.id, items: cart.map(item => ({ productId: item.id, quantity: 1 })) })
        });
        if (response.ok) {
            const result = await response.json();
            showNotification(`Заказ №${result.orderId} оформлен!`);
            cart = [];
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartCount();
            renderCart();
            if (cartModal) cartModal.style.display = 'none';
            loadOrders();
        } else {
            showNotification('Ошибка оформления');
        }
    } catch (error) {
        showNotification('Ошибка подключения');
    }
}

function updateUIForUser(user) {
    if (user) {
        authBtn.style.display = 'none';
        userMenu.style.display = 'flex';
        currentUser = user;
        localStorage.setItem('currentUser', JSON.stringify(user));
        loadOrders();
    } else {
        authBtn.style.display = 'block';
        userMenu.style.display = 'none';
        currentUser = null;
        localStorage.removeItem('currentUser');
        loadOrders();
    }
}

if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const response = await fetch(`${API_URL}/api/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ login: document.getElementById('loginInput').value, password: document.getElementById('passwordInput').value })
        });
        if (response.ok) {
            const user = await response.json();
            updateUIForUser(user);
            modal.style.display = 'none';
            showNotification(`Добро пожаловать, ${user.login}!`);
        } else {
            showNotification('Неверный логин или пароль');
        }
    });
}

logoutBtn?.addEventListener('click', () => { updateUIForUser(null); showNotification('Вы вышли'); });
authBtn?.addEventListener('click', () => modal.style.display = 'flex');
document.querySelectorAll('.modal__close').forEach(btn => btn.addEventListener('click', () => { modal.style.display = 'none'; cartModal.style.display = 'none'; }));
cartBtn?.addEventListener('click', () => { renderCart(); cartModal.style.display = 'flex'; });
clearCartBtn?.addEventListener('click', clearCart);
checkoutBtn?.addEventListener('click', checkout);
window.addEventListener('click', (e) => { if (e.target === modal) modal.style.display = 'none'; if (e.target === cartModal) cartModal.style.display = 'none'; });

const savedUser = localStorage.getItem('currentUser');
if (savedUser) updateUIForUser(JSON.parse(savedUser));
updateCartCount();