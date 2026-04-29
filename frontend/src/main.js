import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { createI18n } from 'vue-i18n';

import App from './App.vue';
import router from './router';

// PrimeVue core + services + directives
import PrimeVue from 'primevue/config';
import ToastService from 'primevue/toastservice';
import DialogService from 'primevue/dialogservice';
import ConfirmationService from 'primevue/confirmationservice';
import BadgeDirective from 'primevue/badgedirective';
import Ripple from 'primevue/ripple';
import StyleClass from 'primevue/styleclass';
import Tooltip from 'primevue/tooltip';

// Editor (kept for renderValue monkey-patch below)
import Editor from 'primevue/editor';

// Custom app components
import Loading from '@/components/Loading.vue';
import NodeGItem from '@/components/NodeGItem.vue';
import ProgressTimeLine from '@/components/ProgressTimeLine.vue';
import AdditionalRequest from '@/components/AdditionalRequest.vue';
import DocumentList from './components/DocumentList.vue';
import AddOrdersShipComp from './components/AddOrdersShipComp.vue';
import DocumentsComp from './components/DocumentsComp.vue';
import FiltersProducts from './components/FiltersProducts.vue';
import DebtCheck from './components/DebtCheck.vue';
import StepHistory from './components/StepHistory.vue';
import AddApprovalersComp from './components/AddApprovalersComp.vue';
import AddApprovalLevelComp from './components/AddApprovalLevelComp.vue';
import PaymentMedComp from './components/PaymentMedComp.vue';
import OutputCheck from './components/OutputCheck.vue';
import CustomerSelector from '@/components/CustomerSelector.vue';
import CustomerSelection from '@/components/CustomerSelection.vue';
import ProductFilter from '@/components/ProductFilter.vue';
import ProductSelector from '@/components/ProductSelector.vue';
import SelectAddress from '@/components/SelectAddress.vue';
import SearchAddress from './components/SearchAddress.vue';
import CustomeMultiSelect from './components/CustomeMultiSelect.vue';
import ButtonGoBack from './components/ButtonGoBack.vue';
import ClaimsComponent from './components/ClaimsComponent.vue';
import PercentBar from './components/PercentBar.vue';
import FilesAttachment from './components/FilesAttachment.vue';
import DocumentAttach from './components/DocumentAttach/DocumentAttach.vue';
import FilePicker from './components/FilePicker/index.vue';
import CheckSuccess from './components/CheckSuccess.vue';
import KiemTraCongNo from './components/KiemTraCongNo/KiemTraCongNo.vue';
import DraggableTree from './components/DraggableTree/DraggableTree.vue';
import ProductCard from './views/client/pages/home/components/ProductsCard.vue';
import UserSelector from './components/UserSelector/UserSelector.vue';
import BillDetail from './components/BillDetail/BillDetail.vue';
import GlobalSearch from './components/GlobalSearch/GlobalSearch.vue';
import AUserSelector from './components/AUserSelector/AUserSelector.vue';
import FileSelect from './components/FileSelect/FileSelect.vue';
import InputOtp from './components/InputOtp/InputOtp.vue';

// Global styles + plugins
import '@fortawesome/fontawesome-free/css/all.css';
import '@/assets/styles.scss';
import '@/assets/main.css';
import notificationPlugin from './services/Notification';
import ConditionHandler from './helpers/filter.helper';
import locale from './services/primevueLocale';

// i18n setup
import vi from './i18n/vi.json';
import en from './i18n/en.json';
const savedLocale = localStorage.getItem('language-pos') || 'vi';
const i18n = createI18n({
    legacy: false,
    locale: savedLocale,
    fallbackLocale: 'vi',
    messages: { vi, en }
});

const app = createApp(App);
const pinia = createPinia();

app.use(i18n);
app.use(router);
app.use(notificationPlugin, { i18n });
app.use(pinia);
app.provide('conditionHandler', new ConditionHandler());
app.use(PrimeVue, { ripple: false, locale });
app.use(ToastService);
app.use(DialogService);
app.use(ConfirmationService);

app.directive('tooltip', Tooltip);
app.directive('badge', BadgeDirective);
app.directive('ripple', Ripple);
app.directive('styleclass', StyleClass);

// Editor with renderValue patch for Quill HTML content
app.component('Editor', Editor);

// Custom global components
app.component('OutputCheck', OutputCheck);
app.component('ProgressTimeLine', ProgressTimeLine);
app.component('AddApprovalersComp', AddApprovalersComp);
app.component('AddApprovalLevelComp', AddApprovalLevelComp);
app.component('PaymentMedComp', PaymentMedComp);
app.component('FiltersProducts', FiltersProducts);
app.component('Loading', Loading);
app.component('NodeGItem', NodeGItem);
app.component('AdditionalRequest', AdditionalRequest);
app.component('DocumentList', DocumentList);
app.component('AddOrdersShipComp', AddOrdersShipComp);
app.component('DocumentsComp', DocumentsComp);
app.component('DebtCheck', DebtCheck);
app.component('StepHistory', StepHistory);
app.component('CustomerSelector', CustomerSelector);
app.component('CustomerSelection', CustomerSelection);
app.component('ProductFilter', ProductFilter);
app.component('ProductSelector', ProductSelector);
app.component('SelectAddress', SelectAddress);
app.component('SearchAddress', SearchAddress);
app.component('CustomeMultiSelect', CustomeMultiSelect);
app.component('ButtonGoBack', ButtonGoBack);
app.component('ClaimsComponent', ClaimsComponent);
app.component('PercentBar', PercentBar);
app.component('FilesAttachment', FilesAttachment);
app.component('DocumentAttach', DocumentAttach);
app.component('FilePicker', FilePicker);
app.component('CheckSuccess', CheckSuccess);
app.component('KiemTraCongNo', KiemTraCongNo);
app.component('DraggableTree', DraggableTree);
app.component('ProductCard', ProductCard);
app.component('UserSelector', UserSelector);
app.component('BillDetail', BillDetail);
app.component('GlobalSearch', GlobalSearch);
app.component('AUserSelector', AUserSelector);
app.component('FileSelect', FileSelect);
app.component('InputOtp', InputOtp);

Editor.methods.renderValue = function renderValue(value) {
    if (this.quill) {
        if (value) {
            const delta = this.quill.clipboard.convert({ html: value });
            this.quill.setContents(delta, 'silent');
        } else {
            this.quill.setText('');
        }
    }
};

app.mount('#app');

// Khởi động SignalR hub cho reference-data cache invalidation.
// Hub tự bỏ qua nếu chưa có token; auth store sẽ start lại sau khi login.
import('./services/referenceDataHub').then(({ startReferenceDataHub }) => {
    startReferenceDataHub();
});
