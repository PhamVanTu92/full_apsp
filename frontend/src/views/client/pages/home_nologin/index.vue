<template>
    <div v-if="true"> 
        <div class="grid">
            <!-- <div class="col-3 relative">
                <div class="sticky" style="top: 135px">
                    <CategoryComponent></CategoryComponent>
                </div>
            </div> -->
            <div class="col-12">
                <ProductList></ProductList>
            </div>
        </div>
    </div>
    <div v-else class="maintenance-container"> 
        <div class="maintenance-content">
            <div class="maintenance-icon">
                <i class="pi pi-cog pi-spin" style="font-size: 4rem; color: #fa8232"></i>
            </div>
            <h1 class="maintenance-title">{{ t('Login.maintenancePage.title') }}</h1>
            <p class="maintenance-description">{{ t('Login.maintenancePage.message') }}</p>

            <div class="maintenance-actions">
                <Button :label="t('Login.buttons.login')" icon="pi pi-sign-in" class="orange-btn maintenance-btn" @click="goToLogin" />
            </div>
            <div class="contact-info">
                <p><i class="pi pi-envelope"></i> {{ t('Login.maintenancePage.contact.email') }}</p>
                <p><i class="pi pi-phone"></i> {{ t('Login.maintenancePage.contact.phone') }}</p>
            </div>
        </div>
    </div>
</template>


<style scoped></style>

<script setup>
import { ref, reactive, computed, onBeforeMount, onBeforeUnmount, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import API from '@/api/api-main';
import { useGlobal } from '@/services/useGlobal';
import CategoryComponent from '../categories/components/CategoryComponent.vue';
import ProductList from '../categories/components/ProductList.vue';
import Button from 'primevue/button';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const router = useRouter();
// -------- Charts --------------------
const countdownlabel = ref();
var interval = null;
const f = (n) => {
    return n < 10 ? '0' + n : n;
};
const doCountdown = () => {
    let times = 7 * 24 * 60 * 60; // days;
    interval = setInterval(() => {
        if (times <= 0) {
            countdownlabel.value = 'Đã kết thúc';
            clearInterval(interval);
            return;
        }
        times--;
        const days = Math.floor(times / (24 * 60 * 60));
        const hours = Math.floor((times % (24 * 60 * 60)) / (60 * 60));
        const minutes = Math.floor((times % (60 * 60)) / 60);
        const seconds = Math.floor(times % 60);
        countdownlabel.value = `${f(days)} ngày : ${f(hours)} giờ : ${f(minutes)} phút : ${f(seconds)} giây`;
    }, 1000);
};
doCountdown();

onBeforeUnmount(() => {
    clearInterval(interval);
});

// Xử lý chuyển hướng đến trang đăng nhập
const goToLogin = () => {
    router.push('/login');
};

// Xử lý liên hệ hỗ trợ
const contactSupport = () => {
    // Có thể mở modal hoặc chuyển hướng đến trang liên hệ
    window.open('mailto:support@saigonpetro.com', '_blank');
};
</script>

<style scoped>
.orange-btn:hover {
    background-color: #ffb07b;
    border: #ffb07b;
}

.orange-btn {
    background-color: #fa8232;
    border: #fa8232;
    padding-top: 1rem !important;
    padding-bottom: 1rem !important;
}

.card-hover:hover {
    transition: transform 0.3s ease-in-out;
    transform: scale(1.02);
    box-shadow: 0 5px 10px rgba(0, 0, 0, 0.1);
    cursor: pointer;
}

.item-container {
    position: relative;
    overflow: hidden;
}

.image-wrapper {
    position: relative;
}

.image-wrapper img {
    display: block;
    transition: all 0.3s ease;
}

.image-wrapper .overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.2);
    opacity: 0;
    transition: opacity 0.3s ease;
}

.item-container:hover .image-wrapper .overlay {
    opacity: 1;
}

.hidden-text {
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
    text-overflow: ellipsis;
}

.hidden-product-categories {
    white-space: nowrap;
    /* Ngăn văn bản xuống dòng */
    overflow: hidden;
    /* Ẩn phần văn bản vượt quá kích thước của phần tử */
    text-overflow: ellipsis;
    /* Hiển thị dấu ... khi văn bản bị cắt */
    max-width: 100%;
}

.sale-absolute {
    border-color: #f2ee1b;
    margin-left: 295px;
    margin-bottom: 240px;
    background-color: #00a59b;
}

/* Maintenance Screen Styles */
.maintenance-container {
    display: flex;
    align-items: center;
    justify-content: center;

    padding: 2rem;
}

.maintenance-content {
    text-align: center;
    background: white;
    padding: 3rem;
    border-radius: 20px;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
    max-width: 600px;
    width: 100%;
    animation: fadeInUp 0.8s ease-out;
}

@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.maintenance-icon {
    margin-bottom: 2rem;
}

.maintenance-title {
    color: #2c3e50;
    font-size: 2.5rem;
    font-weight: 700;
    margin-bottom: 1rem;
    background: linear-gradient(45deg, #fa8232, #ff6b35);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.maintenance-description {
    color: #64748b;
    font-size: 1.1rem;
    line-height: 1.6;
    margin-bottom: 2rem;
    max-width: 480px;
    margin-left: auto;
    margin-right: auto;
}

.maintenance-actions {
    display: flex;
    gap: 1rem;
    justify-content: center;
    margin-bottom: 2rem;
    flex-wrap: wrap;
}

.maintenance-btn {
    padding: 0.75rem 2rem !important;
    border-radius: 50px !important;
    font-weight: 600 !important;
    transition: all 0.3s ease !important;
    min-width: 180px;
}

.maintenance-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
}

.contact-info {
    border-top: 1px solid #e2e8f0;
    padding-top: 2rem;
    margin-top: 2rem;
}

.contact-info p {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    color: #64748b;
    margin-bottom: 0.5rem;
    font-size: 0.95rem;
}

.contact-info i {
    color: #fa8232;
}

/* Responsive Design */
@media (max-width: 768px) {
    .maintenance-container {
        padding: 1rem;
    }

    .maintenance-content {
        padding: 2rem 1.5rem;
    }

    .maintenance-title {
        font-size: 2rem;
    }

    .maintenance-actions {
        flex-direction: column;
        align-items: center;
    }

    .maintenance-btn {
        width: 100%;
        max-width: 280px;
    }
}
</style>
