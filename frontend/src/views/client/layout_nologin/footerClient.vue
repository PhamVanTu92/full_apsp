<template>
    <div v-if="0" class="green-bg text-white mt-8 text-res">
        <div class="grid type-layout py-7 px-8 mx-auto">
            <div class="col flex flex-column">
                <router-link :to="`/client`">
                    <div class="flex">
                        <img src="/image/logo/logoNew.png" class="" alt="logo" />
                    </div>
                </router-link>
                <ul>
                    <li>
                        <span>Customer Supports: </span>
                        <span class="text-base text-white mt-2">1900 0104</span>
                    </li>
                    <li>Lầu1, 6B Tôn Đức Thắng, P.Bến Nghé, Q.1, TP.HCM</li>
                    <li>info@apsaigonpetro.com.vn</li>
                </ul>
            </div>
            <div class="col flex flex-column">
                <strong class="uppercase">Danh mục</strong>
                <ul>
                    <li>Saigon Petro</li>
                    <li>AP Oil</li>
                    <li>Sino</li>
                    <li class="flex align-items-center">
                        <svg width="24" height="2" viewBox="0 0 24 2" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <line x1="1" y1="1" x2="23" y2="1" stroke="#F2EE1B" stroke-width="2" stroke-linecap="round" />
                        </svg>
                        <span class="ml-2">Singtec Oil</span>
                    </li>
                    <li>Polaris</li>
                    <li>Export & Toll Blending</li>
                    <li>
                        <Button severity="success" class="w-13rem" icon="" :label="t('client.view_all')" />
                    </li>
                </ul>
            </div>
            <div class="col flex flex-column">
                <strong class="uppercase">{{ t('Footer.linkProduct.linkQuicks') }}</strong>
                <ul>
                    <li class="text-white">{{ t('Footer.linkProduct.product') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.cart') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.favorite') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.compare') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.trackOrder') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.customerCare') }}</li>
                    <li class="text-white">{{ t('Footer.linkProduct.aboutUs') }}</li>
                </ul>
            </div>
        </div>
        <div class="flex justify-content-center h-4rem align-items-center border-top-1 border-gray-800">
            <span class="text-white-alpha-40 lg:text-base sm:text-xs">Cổng thông tin mua sắm của AP Saigon Petro © 2024. Design by FoxAi</span>
        </div>
    </div>
    <div v-if="1" class="bg-green-800 py-8">
        <div class="content-container mx-auto px-4">
            <div class="grid grid-nogutter">
                <div class="col-12 md:col-6 lg:col-3 footer-column" v-if="0">
                    <iframe
                        src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.466483948042!2d106.70784998995323!3d10.775539324753522!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f45c8de0103%3A0xb26f32a172fa078d!2zMSBUw7RuIMSQ4bupYyBUaOG6r25nLCBC4bq_biBOZ2jDqSwgUXXhuq1uIDEsIEjhu5MgQ2jDrSBNaW5oLCBWaeG7h3QgTmFt!5e0!3m2!1svi!2s!4v1743647080513!5m2!1svi!2s"
                        height="300"
                        style="border: 0"
                        loading="lazy"
                        referrerpolicy="no-referrer-when-downgrade"
                    ></iframe>
                </div>
                <div class="col-12 md:col-6 lg:col-3 footer-column">
                    <div class="footer-section">
                        <router-link :to="`/client`" class="footer-logo-link">
                            <img src="/src/assets/images/new-logo-ap.png" class="footer-logo" alt="logo" />
                        </router-link>
                        <ul class="footer-list company-info">
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.name') }}:</strong> CÔNG TY CỔ PHẦN AP SAIGON PETRO
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.internationalName') }}:</strong> AP SAIGON PETRO JOINT STOCK COMPANY
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.shortName') }}:</strong> AP SAIGON PETRO JSC
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.address') }}:</strong> Lầu 1, 6B Tôn Đức Thắng, Phường Sài Gòn, Thành phố Hồ Chí Minh, Việt Nam
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.phone') }}:</strong> 028 6678 0245
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.email') }}:</strong> info@apsaigonpetro.com.vn
                            </li>
                            <li class="company-info-item">
                                <strong>{{ t('Footer.company.website') }}:</strong>
                                <a class="footer-link" href="https://portal.apsaigonpetro.com" target="_blank"> portal.apsaigonpetro.com </a>
                            </li>
                        </ul>
                        <a :href="`http://online.gov.vn/Home/WebDetails/134730`" class="footer-logo-link">
                            <img src="/src/assets/images/logo-da-thong-bao-bo-cong-thuong.png" class="footer-logo" alt="logo" />
                        </a>
                    </div>
                </div>
                <div class="col-12 md:col-6 lg:col-3 footer-column">
                    <div class="footer-section">
                        <h3 class="footer-title">{{ t('client.additional_information') }}</h3>
                        <ul class="footer-list">
                            <li class="footer-item">
                                <router-link to="#" class="footer-link"
                                    ><strong>{{ t('Footer.additionalInfo.taxCode') }}:</strong> 0305715556
                                </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.additionalInfo.firstRegistration') }}: 06/05/2008</router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.additionalInfo.lastChange') }}: 19/07/2022</router-link>
                            </li>
                            <li class="footer-item">
                                <span class="footer-text"
                                    ><strong>{{ t('Footer.additionalInfo.legalRepresentative') }}:</strong></span
                                >
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.additionalInfo.name') }}</router-link>
                            </li>
                        </ul>
                    </div>
                </div>
                <div class="col-12 md:col-6 lg:col-3 footer-column">
                    <div class="footer-section">
                        <h3 class="footer-title">{{ t('Footer.products[6]') }}</h3>
                        <ul class="footer-list">
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[0]') }}</router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[1]') }}</router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[2]') }}</router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[3]') }}</router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[4]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="#" class="footer-link">{{ t('Footer.products[5]') }}</router-link>
                            </li>
                        </ul>
                        <div class="mt-4">
                            <Button :label="t('client.view_all')" severity="success" class="footer-button" />
                        </div>
                    </div>
                </div>
                <div class="col-12 md:col-6 lg:col-3 footer-column">
                    <div class="footer-section">
                        <h3 class="footer-title">{{ t('Footer.linkProduct.linkQuicks') }}</h3>
                        <ul class="footer-list">
                            <li class="footer-item">
                                <router-link to="/preview-page/gioi-thieu-doanh-nghiep" class="footer-link"> {{ t('Footer.quickLinks[0]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-giao-dich-chung" class="footer-link"> {{ t('Footer.quickLinks[1]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-doi-tra-bao-hanh" class="footer-link"> {{ t('Footer.quickLinks[2]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-khieu-nai-ho-tro" class="footer-link"> {{ t('Footer.quickLinks[3]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-bao-ve-thong-tin-ca-nhan" class="footer-link"> {{ t('Footer.quickLinks[4]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-van-chuyen-giao-nhan" class="footer-link"> {{ t('Footer.quickLinks[5]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/chinh-sach-thanh-toan" class="footer-link"> {{ t('Footer.quickLinks[6]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/thong-tin-hang-hoa-dich-vu" class="footer-link"> {{ t('Footer.quickLinks[7]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/huong-dan-dat-hang" class="footer-link"> {{ t('Footer.quickLinks[8]') }} </router-link>
                            </li>
                            <li class="footer-item">
                                <router-link to="/preview-page/dieu-khoan-su-dung" class="footer-link"> {{ t('Footer.quickLinks[9]') }} </router-link>
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="footer-bottom">
        <div class="content-container mx-auto px-4">
            <div class="text-center">
                <span class="footer-bottom-text">
                    {{ t('Footer.footer.copyright') }} {{ new Date().getFullYear() }}. {{ t('Footer.footer.designBy') }}
                    <a href="https://fox.ai.vn" target="_blank" class="footer-bottom-link">{{ t('Footer.footer.FoxAi') }}</a>
                </span>
            </div>
        </div>
    </div>
</template>
<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const openFile = (filePath) => {
    window.open(filePath, '_blank');
};
</script>
<style scoped>
/* Footer Layout */
.footer-column {
    padding: 0 1.5rem;
    margin-bottom: 2rem;
}

.footer-section {
    height: 100%;
    display: flex;
    flex-direction: column;
}

/* Logo */
.footer-logo-link {
    display: inline-block;
    margin-bottom: 1.5rem;
}

.footer-logo {
    height: 3.5rem;
    width: auto;
    transition: opacity 0.3s ease;
}

.footer-logo:hover {
    opacity: 0.8;
}

/* Typography */
.footer-title {
    font-size: 1.25rem;
    font-weight: 700;
    color: #f8fafc;
    margin-bottom: 1.5rem;
    padding-bottom: 0.5rem;
    display: inline-block;
}

/* Lists */
.footer-list {
    list-style-type: none;
    padding: 0;
    margin: 0;
    flex-grow: 1;
}

.footer-item {
    margin-bottom: 1rem;
    line-height: 1.6;
}

.company-info .company-info-item {
    margin-bottom: 0.875rem;
    line-height: 1.7;
}

/* Links and Text */
.footer-link {
    color: #e2e8f0;
    text-decoration: none;
    transition: all 0.3s ease;
    display: inline-block;
    position: relative;
}

.footer-link:hover {
    color: #fdef2e;
    transform: translateX(4px);
}

.footer-text {
    color: #e2e8f0;
    line-height: 1.6;
}

/* Company Info Specific Styling */
.company-info-item {
    color: #e2e8f0;
    line-height: 1.7;
}

.company-info-item strong {
    color: #f8fafc;
    font-weight: 600;
}

/* Button */
.footer-button {
    width: 100%;
    max-width: 200px;
    padding: 0.75rem 1.5rem;
    font-weight: 600;
    border-radius: 8px;
    transition: all 0.3s ease;
}

.footer-button:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

/* Footer Bottom */
.footer-bottom {
    background-color: #064e3b;
    padding: 1.5rem 0;
    border-top: 1px solid #065f46;
}

.footer-bottom-text {
    color: #d1d5db;
    font-size: 0.875rem;
    line-height: 1.5;
}

.footer-bottom-link {
    color: #fdef2e;
    font-weight: 600;
    text-decoration: none;
    transition: color 0.3s ease;
}

.footer-bottom-link:hover {
    color: #ffffff;
    text-decoration: underline;
}

/* Responsive Design */
@media (max-width: 1024px) {
    .footer-column {
        padding: 0 1rem;
        margin-bottom: 3rem;
    }

    .footer-title {
        font-size: 1.125rem;
        margin-bottom: 1.25rem;
    }
}

@media (max-width: 768px) {
    .footer-column {
        padding: 0 0.75rem;
        margin-bottom: 2.5rem;
    }

    .footer-title {
        font-size: 1rem;
        margin-bottom: 1rem;
    }

    .footer-item {
        margin-bottom: 0.875rem;
    }

    .footer-button {
        width: 100%;
        max-width: none;
    }

    .footer-bottom-text {
        font-size: 0.8rem;
        text-align: center;
    }
}

@media (max-width: 480px) {
    .footer-column {
        padding: 0 0.5rem;
    }

    .footer-logo {
        height: 3rem;
    }

    .footer-title {
        font-size: 0.95rem;
    }

    .footer-item {
        font-size: 0.9rem;
    }
}

/* Content Container */
.content-container {
    max-width: 1350px;
}

/* Accessibility */
@media (prefers-reduced-motion: reduce) {
    .footer-link,
    .footer-logo,
    .footer-button {
        transition: none;
    }

    .footer-link:hover {
        transform: none;
    }

    .footer-button:hover {
        transform: none;
    }
}
</style>
