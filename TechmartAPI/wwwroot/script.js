// Базовый URL API
const API_URL = window.location.origin;

// Состояние
let currentUser = null;
let cart = JSON.parse(localStorage.getItem('cart')) || [];

// DOM элементы
const authBtn = document.getElementById('authBtn');
const userAvatar = document.getElementById('userAvatar');
const userMenu = document.getElementById('userMenu');
const logoutBtn = document.getElementById('logoutBtn');
const cartCountSpan = document.getElementById('cartCount');
const loginForm = document.getElementById('loginForm');
const modal = document.getElementById('authModal');
const cartBtn = document.getElementById('cartBtn');
const cartModal = document.getElementById('cartModal');
const cartModalClose = document.getElementById('cartModalClose');
const cartItemsList = document.getElementById('cartItemsList');
const cartTotalSpan = document.getElementById('cartTotal');
const clearCartBtn = document.getElementById('clearCartBtn');
const checkoutBtn = document.getElementById('checkoutBtn');

// Функция получения цвета для категории
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

// Отображение товаров на главной
function renderProducts(containerId, products) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    if (!products || products.length === 0) {
        container.innerHTML = '<div class="loading">Нет товаров</div>';
        return;
    }
    
    container.innerHTML = products.map(p => `
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
    
    document.querySelectorAll(`#${containerId} .add-to-cart`).forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = parseInt(btn.dataset.id);
            addToCartById(id);
        });
    });
}

// Отображение корзины
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
        return `
            <div style="display: flex; justify-content: space-between; align-items: center; padding: 12px; border-bottom: 1px solid #eee;">
                <div style="flex: 2;">
                    <div style="font-weight: 600;">${p.name}</div>
                    <div style="font-size: 12px; color: #666;">${p.category}</div>
                </div>
                <div style="flex: 1; text-align: right; font-weight: 600;">${Number(p.price).toLocaleString()} ₽</div>
                <button class="remove-from-cart" data-index="${index}" style="background: none; border: none; color: #D96E6E; font-size: 20px; cursor: pointer; margin-left: 12px;">&times;</button>
            </div>
        `;
    }).join('');
    
    if (cartTotalSpan) {
        cartTotalSpan.innerHTML = `Итого: ${total.toLocaleString()} ₽`;
    }
    
    document.querySelectorAll('.remove-from-cart').forEach(btn => {
        btn.addEventListener('click', () => {
            const index = parseInt(btn.dataset.index);
            removeFromCart(index);
        });
    });
}

// Удаление товара из корзины
function removeFromCart(index) {
    cart.splice(index, 1);
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
    renderCart();
    updateRecommendations();
    showNotification('Товар удалён из корзины');
}

// Очистка корзины
function clearCart() {
    if (cart.length === 0) {
        showNotification('Корзина уже пуста');
        return;
    }
    cart = [];
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
    renderCart();
    updateRecommendations();
    showNotification('Корзина очищена');
}

// Получение всех товаров
async function getAllProducts() {
    const response = await fetch(`${API_URL}/api/Products`);
    return response.json();
}

// Получение товаров, которые покупают вместе
async function getTogetherProducts(productId) {
    try {
        const response = await fetch(`${API_URL}/api/Recommendations/together/${productId}`);
        return response.json();
    } catch (e) {
        return [];
    }
}

// Получение популярных товаров
async function getPopularProducts() {
    try {
        const response = await fetch(`${API_URL}/api/Recommendations/popular`);
        return response.json();
    } catch (e) {
        return [];
    }
}

// Рекомендации на основе корзины
async function getCartBasedRecommendations() {
    if (cart.length === 0) {
        return await getPopularProducts();
    }
    
    const togetherProductsSet = new Set();
    const cartProductIds = cart.map(p => p.id);
    
    for (const product of cart) {
        const together = await getTogetherProducts(product.id);
        together.forEach(t => {
            if (!cartProductIds.includes(t.id)) {
                togetherProductsSet.add(JSON.stringify(t));
            }
        });
    }
    
    const recommendations = Array.from(togetherProductsSet).map(s => JSON.parse(s));
    
    if (recommendations.length === 0) {
        return await getPopularProducts();
    }
    
    return recommendations.slice(0, 8);
}

// Добавление в корзину
async function addToCartById(productId) {
    try {
        const allProducts = await getAllProducts();
        const product = allProducts.find(p => p.id === productId);
        if (product) {
            cart.push(product);
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartCount();
            renderCart();
            showNotification('Товар добавлен в корзину');
            await updateRecommendations();
        }
    } catch (error) {
        console.error('Ошибка добавления:', error);
        showNotification('Ошибка');
    }
}

function updateCartCount() {
    if (cartCountSpan) {
        cartCountSpan.textContent = cart.length;
    }
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

// Обновление всех рекомендаций
async function updateRecommendations() {
    console.log('Обновление рекомендаций на основе корзины...');
    
    const cartRecs = await getCartBasedRecommendations();
    renderProducts('personalRecs', cartRecs);
    
    if (cart.length > 0) {
        const together = await getTogetherProducts(cart[0].id);
        renderProducts('togetherRecs', together);
    } else {
        const popular = await getPopularProducts();
        renderProducts('togetherRecs', popular);
    }
    
    const popular = await getPopularProducts();
    renderProducts('popularRecs', popular);
}

// Добавление ссылки на админ-панель для администратора
function addAdminLink() {
    const nav = document.querySelector('.nav');
    if (nav && !document.getElementById('adminLink')) {
        const adminLink = document.createElement('a');
        adminLink.id = 'adminLink';
        adminLink.href = 'admin.html';
        adminLink.className = 'nav__link';
        adminLink.textContent = 'Админ-панель';
        nav.appendChild(adminLink);
        console.log('Ссылка на админ-панель добавлена');
    }
}

function removeAdminLink() {
    const adminLink = document.getElementById('adminLink');
    if (adminLink) adminLink.remove();
}

// Авторизация
function updateUIForUser(user) {
    console.log('Авторизован пользователь:', user);
    console.log('Роль пользователя:', user ? user.role : 'null');
    
    if (user) {
        if (authBtn) authBtn.style.display = 'none';
        if (userMenu) userMenu.style.display = 'flex';
        currentUser = user;
        localStorage.setItem('currentUser', JSON.stringify(user));
        
        const adminRoles = ['администратор', 'admin', 'Администратор', 'ADMIN'];
        if (adminRoles.includes(user.role)) {
            console.log('Администратор обнаружен, добавляем ссылку');
            addAdminLink();
        } else {
            console.log('Обычный пользователь, роль:', user.role);
            removeAdminLink();
        }
    } else {
        if (authBtn) authBtn.style.display = 'block';
        if (userMenu) userMenu.style.display = 'none';
        currentUser = null;
        localStorage.removeItem('currentUser');
        removeAdminLink();
    }
}

// Функция выхода
function logout() {
    currentUser = null;
    localStorage.removeItem('currentUser');
    updateUIForUser(null);
    updateRecommendations();
    showNotification('Вы вышли из аккаунта');
}

if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const login = document.getElementById('loginInput').value;
        const password = document.getElementById('passwordInput').value;
        
        try {
            const response = await fetch(`${API_URL}/api/Auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ login, password })
            });
            
            if (response.ok) {
                const user = await response.json();
                updateUIForUser(user);
                if (modal) modal.style.display = 'none';
                await updateRecommendations();
                showNotification(`Добро пожаловать, ${user.login}!`);
            } else {
                showNotification('Неверный логин или пароль');
            }
        } catch (error) {
            console.error('Ошибка авторизации:', error);
            showNotification('Ошибка подключения к серверу');
        }
    });
}

// Кнопка выхода
if (logoutBtn) {
    logoutBtn.addEventListener('click', (e) => {
        e.preventDefault();
        logout();
    });
}

// Модальное окно авторизации
if (authBtn) {
    authBtn.addEventListener('click', () => {
        if (modal) modal.style.display = 'flex';
    });
}

document.querySelector('.modal__close')?.addEventListener('click', () => {
    if (modal) modal.style.display = 'none';
});

window.addEventListener('click', (e) => {
    if (e.target === modal) modal.style.display = 'none';
});

// Модальное окно корзины
if (cartBtn) {
    cartBtn.addEventListener('click', (e) => {
        e.preventDefault();
        renderCart();
        if (cartModal) cartModal.style.display = 'flex';
    });
}

if (cartModalClose) {
    cartModalClose.addEventListener('click', () => {
        if (cartModal) cartModal.style.display = 'none';
    });
}

if (clearCartBtn) {
    clearCartBtn.addEventListener('click', () => {
        clearCart();
    });
}

// Оформление заказа с сохранением в БД
if (checkoutBtn) {
    checkoutBtn.addEventListener('click', async () => {
        if (cart.length === 0) {
            showNotification('Корзина пуста');
            return;
        }
        
        if (!currentUser) {
            showNotification('Необходимо войти в аккаунт');
            if (modal) modal.style.display = 'flex';
            return;
        }
        
        const originalText = checkoutBtn.textContent;
        checkoutBtn.textContent = 'Оформление...';
        checkoutBtn.disabled = true;
        
        try {
            const orderData = {
                userId: currentUser.id,
                items: cart.map(item => ({
                    productId: item.id,
                    quantity: 1
                }))
            };
            
            const response = await fetch(`${API_URL}/api/Orders/create`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(orderData)
            });
            
            if (response.ok) {
                const result = await response.json();
                showNotification(`Заказ №${result.orderId} оформлен! Спасибо за покупку!`);
                
                cart = [];
                localStorage.setItem('cart', JSON.stringify(cart));
                updateCartCount();
                renderCart();
                updateRecommendations();
                if (cartModal) cartModal.style.display = 'none';
            } else {
                const error = await response.json();
                showNotification(error.message || 'Ошибка оформления заказа');
            }
        } catch (error) {
            console.error('Ошибка оформления заказа:', error);
            showNotification('Ошибка подключения к серверу');
        } finally {
            checkoutBtn.textContent = originalText;
            checkoutBtn.disabled = false;
        }
    });
}

window.addEventListener('click', (e) => {
    if (e.target === cartModal) {
        if (cartModal) cartModal.style.display = 'none';
    }
});

// Восстановление сессии
const savedUser = localStorage.getItem('currentUser');
if (savedUser) {
    currentUser = JSON.parse(savedUser);
    updateUIForUser(currentUser);
}

// Инициализация
updateCartCount();
renderCart();
updateRecommendations();