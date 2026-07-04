const API_URL = window.location.origin;
let currentUser = null;
let allProducts = [];
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
const catalogGrid = document.getElementById('catalogGrid');
const searchInput = document.getElementById('searchInput');
const categoryFilter = document.getElementById('categoryFilter');

function getCategoryColor(category) {
    const colors = {
        'Электроника': '#4A90D9',
        'Аксессуары': '#E89139',
        'Комплектующие': '#2E9B6E',
        'Сумки': '#9B6EBF',
        'Мебель': '#D96E6E'
    };
    return colors[category] || '#6B6B6B';
}

function renderProducts(products) {
    if (!products || products.length === 0) {
        catalogGrid.innerHTML = '<div class="loading">Нет товаров</div>';
        return;
    }
    catalogGrid.innerHTML = products.map(p => `
        <div class="product-card" data-id="${p.id}">
            <div style="width:100%; height:180px; background:${getCategoryColor(p.category)}; display:flex; align-items:center; justify-content:center; color:white; font-size:18px; font-weight:500;">
                ${p.category}
            </div>
            <div class="product-card__info">
                <div class="product-card__title">${p.name}</div>
                <div class="product-card__category">${p.category}</div>
                <div class="product-card__price">${Number(p.price).toLocaleString()} ₽</div>
                <button class="add-to-cart" data-id="${p.id}">В корзину</button>
            </div>
        </div>
    `).join('');
    document.querySelectorAll('.add-to-cart').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = parseInt(btn.dataset.id);
            addToCartById(id);
        });
    });
}

async function loadProducts() {
    try {
        const response = await fetch(`${API_URL}/api/Products`);
        allProducts = await response.json();
        renderProducts(allProducts);
        renderCategoryFilters();
    } catch (error) {
        console.error('Ошибка загрузки товаров:', error);
    }
}

function renderCategoryFilters() {
    const categories = [...new Set(allProducts.map(p => p.category))];
    categoryFilter.innerHTML = '<button class="category-btn active" data-category="all">Все</button>' +
        categories.map(cat => `<button class="category-btn" data-category="${cat}">${cat}</button>`).join('');
    document.querySelectorAll('.category-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const category = btn.dataset.category;
            if (category === 'all') {
                renderProducts(allProducts);
            } else {
                renderProducts(allProducts.filter(p => p.category === category));
            }
        });
    });
}

searchInput.addEventListener('input', (e) => {
    const query = e.target.value.toLowerCase();
    const filtered = allProducts.filter(p => p.name.toLowerCase().includes(query));
    renderProducts(filtered);
});

async function addToCartById(productId) {
    const product = allProducts.find(p => p.id === productId);
    if (product) {
        cart.push(product);
        localStorage.setItem('cart', JSON.stringify(cart));
        updateCartCount();
        showNotification('Товар добавлен в корзину');
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
    } else {
        authBtn.style.display = 'block';
        userMenu.style.display = 'none';
        currentUser = null;
        localStorage.removeItem('currentUser');
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
loadProducts();