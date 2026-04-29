<template>
    <div>
        <div class="flex justify-content-between mb-4 align-items-center">
            <h4 class="mb-0 font-bold">{{ t('ChangePoint.detailTitle') }}</h4>
            <div class="flex gap-2">
                <ButtonGoBack v-if="!props.isClient" />
            </div>
        </div>
        <ExportFiles :data="props.isClient" ref="ExportFileRef" :type="'VPKM'" />
        <div>
            <CancelReason :isClient="props.isClient" />
            <Header />
            <div class="card">
                <ProductList />
                <div class="grid mt-3">
                    <div class="col-12 md:col-12 flex flex-column gap-3">
                        <Promotion />
                        <Notes :is-client="props.isClient" />
                    </div>
                </div>
                <Buttons :is-client="props.isClient" />
            </div>
            <div class="card">
                <ChungTuGiaoHang :is-client="props.isClient" />
            </div>
            <div class="card mb-7">
                <ThongTin />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import Header from './componentsVPKM/Header.vue';
import ProductList from './componentsVPKM/ProductList.vue';
import Promotion from './componentsVPKM/Promotion.vue';
import Notes from './componentsVPKM/Notes.vue';
import Buttons from './componentsVPKM/Buttons.vue';
import ChungTuGiaoHang from './componentsVPKM/ChungTuGiaoHang.vue';
import ThongTin from './componentsVPKM/ThongTin.vue';
import ExportFiles from './dialogs/ExportFiles.vue';
import CancelReason from './components/CancelReason.vue';
import { useOrderDetailStore } from './store/orderDetail';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const odStore = useOrderDetailStore();
const ExportFileRef = ref();
const props = defineProps({
    isClient: {
        type: Boolean,
        required: false,
        default: false
    }
});

const route = useRoute();
const initialComponent = () => {
    const orderId = route.params.id;
    if (orderId && typeof orderId === 'string' && isInteger(orderId)) {
        odStore.fetchStoreVPKM(Number.parseInt(orderId));
    } else {
        odStore.error = true;
    }
};

function isInteger(value: any) {
    return /^\d+$/.test(value);
} 

watch(
    () => route.params.id,
    (newId) => {
        if (newId && typeof newId === 'string' && isInteger(newId)) {
            odStore.fetchStoreVPKM(Number.parseInt(newId));
        } else {
            odStore.error = true;
        }
    }
);
onMounted(function () {
    initialComponent();
}); 
</script>
