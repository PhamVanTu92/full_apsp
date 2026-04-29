<template>
    <div class="flex flex-column">
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('PromotionalItems.PointConversion.title') }}</h4>
            <div class="flex gap-2">
                <Button @click="router.back()" :label="t('body.promotion.back_button')" icon="pi pi-arrow-left"
                    severity="secondary"/>
                <Button :label="t('body.systemSetting.save_button')" icon="pi pi-save" @click="submitData()"/>
            </div>
        </div>
        <div class="card p-3 mb-3">
            <div class="flex justify-content-between gap-4 flex">
                <div class=" align-items-center">
                    <div class="flex gap-3 align-items-center">
                        <span class="whitespace-nowrap font-semibold w-10rem">{{ t('body.OrderList.customer') }}
                            :</span>
                        <CustomerSelector @clear="() => { arrayProduct = [] }" :width="'30rem'" :filted="true"
                            @item-select="handleCustomerSelect"
                            :placeholder="t('body.OrderList.select_customer_placeholder')" />
                    </div>
                    <div class="flex gap-3 align-items-center mt-2">
                        <span class="whitespace-nowrap font-semibold w-10rem">{{ t('body.OrderList.order_date_label')
                        }}:</span>
                        <Calendar v-model="dataRequest.redeemDate" :minDate="new Date()" showTime hourFormat="24" />
                    </div>
                </div>
                <div class="text-center">
                    <div class="border rounded p-1 inline-block">
                        <p class="text-650 mb-0">{{ t('PromotionalItems.PointConversion.currentPoints') }}</p>
                        <p class="text-red-600 text-2xl font-bold mb-0"> {{ formatNumber(pointUser) }} </p>
                        <p class="text-sm text-gray-500 mb-0">{{ t('body.promotion.voucher_expiry_date_label') }}:</p>
                    </div>
                </div>
            </div>
        </div>
        <div class="card p-3 mb-3">
            <DataTable :value="arrayProduct" dataKey="id" showGriditemDetail stripedRows class="mb-2">
                <Column field="index" header="#" style="width: 3rem">
                    <template #body="{ index }">
                        {{ index + 1 }}
                    </template>
                </Column>
                <Column field="itemName" :header="t('body.report.table_header_product_name_2')">
                    <template #body="{ data }">
                        <div class="flex align-items-center gap-2">
                            <img v-if="data.itM1?.[0]?.filePath" :src="data.itM1[0].filePath" alt=""
                                class="w-4rem h-4rem" />
                            <i v-else class="pi pi-box w-4rem h-4rem text-6xl text-gray-400" />
                            {{ data.itemName }}

                        </div>
                    </template>
                </Column>
                <Column field="exchangePoint" :header="t('PromotionalItems.PointConversion.point1dv')">
                    <template #body="{ data }">
                        {{ formatNumber(data.exchangePoint) }}
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_quantity')">
                    <template #body="{ data }">
                        <InputNumber v-model="data.quantity" :min="1" :max="1000" />
                    </template>
                </Column>
                <Column :header="t('PromotionalItems.PointConversion.table_header_temporary_points')">
                    <template #body="{ data }">
                        {{ formatNumber((data.quantity || 0) * (data.exchangePoint || 0)) }}
                    </template>
                </Column>
                <Column :header="t('body.systemSetting.action')">
                    <template #body="{ data }">
                        <div class="flex justify-content-center">
                            <Button icon="pi pi-trash" severity="danger" text @click="removeProduct(data.id)" />
                        </div>
                    </template>
                </Column>
            </DataTable>
            <div class="w-full flex justify-content-between">
                <div class="mb-3">
                    <ProductSelector v-if="cardCode" :label="t('client.input_product')" icon="" :activeRowPoint="true"
                        andPointApi="Item/ItemPromotion" @confirm="confirmAddProduct($event)" :customer="{
                            id: cardCode
                        }" />
                </div>
                <div class="flex-column">
                    <div class="flex justify-content-center font-bold">
                        {{ t('PromotionalItems.PointConversion.totalPoints') }}: <span class="ml-2">{{
                            formatNumber(totalPoint) || 0
                        }}</span>
                    </div>
                    <p class="text-red-600 text-sm mt-1" v-if="totalPoint > pointUser">{{
                        t('PromotionalItems.PointConversion.maxPoint') }}</p>
                </div>
            </div>
        </div>

        <div class="card p-3 mb-3">
            <span class="block font-semibold mb-1">{{ t('PromotionalItems.PointConversion.note') }}</span>
            <Textarea rows="2" v-model="dataRequest.note" class="w-full" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { ref, computed, } from "vue";
import { useRouter, } from "vue-router";
import { useToast } from 'primevue/usetoast';
import API from "@/api/api-main";
const toast = useToast();
const router = useRouter();
const pointUser = ref();
const { t } = useI18n();
const cardCode = ref();
const arrayProduct = ref<any[]>([]);
const dataRequest = ref({
    "id": 0,
    "customerId": 0,
    "customerCode": "",
    "customerName": "",
    "redeemDate": new Date(),
    "note": "",
    "docType": "DVP",
    "itemDetail": <any[]>[]
})
const totalPoint = computed(() => {
    return arrayProduct.value.reduce((sum: number, product: { quantity: number; exchangePoint: number; }) => {
        return sum + ((product.quantity || 0) * (product.exchangePoint || 0));
    }, 0);
});
const formatNumber = (num: number) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};
const removeProduct = (id: number | string) => {
    arrayProduct.value = arrayProduct.value.filter(p => p.id !== id)
}

const handleCustomerSelect = (event: any) => {
    cardCode.value = event.id;
    dataRequest.value.customerId = event.id;
    dataRequest.value.customerCode = event.cardCode;
    dataRequest.value.customerName = event.cardName;
    pointUser.value = event.customerPoints?.reduce((sum: number, item: any) => sum + (item.remainingPoint || 0), 0);
    arrayProduct.value = [];
};

const validateForm = () => {
    if (!cardCode.value) {
        toast.add({ severity: 'warn', summary: t('validation.title'), detail: t('validation.customer_required'), life: 3000 });
        return false;
    }
    if (!dataRequest.value.redeemDate) {
        toast.add({ severity: 'warn', summary: t('validation.title'), detail: t('validation.redeem_date_required'), life: 3000 });
        return false;
    }
    if (!arrayProduct.value || arrayProduct.value.length === 0) {
        toast.add({ severity: 'warn', summary: t('validation.title'), detail: t('validation.products_required'), life: 3000 });
        return false;
    }
    for (let index = 0; index < arrayProduct.value.length; index++) {
        const line = arrayProduct.value[index];
        if (!line.quantity || line.quantity <= 0) {
            toast.add({ severity: 'warn', summary: t('validation.title'), detail: t('validation.quantity_required', { index: index + 1 }), life: 3000 });
            return false;
        }
    }
    if (totalPoint.value > pointUser.value) {
        toast.add({ severity: 'warn', summary: t('validation.title'), detail: t('PromotionalItems.PointConversion.maxPoint'), life: 3000 });
        return false;
    }
    return true;
};
const submitData = async () => {
    try {
        if (!validateForm()) return
        dataRequest.value.itemDetail = arrayProduct.value.map(line => ({
            itemId: line.id,
            itemCode: line.itemCode,
            itemName: line.itemName,
            quantity: line.quantity,
            pointPerUnit: line.exchangePoint
        }));
        const res = dataRequest.value.id
            ? await API.update(`Redeem/${dataRequest.value.id}`, dataRequest.value)
            : await API.add("Redeem", dataRequest.value);
        toast.add({ severity: 'success', summary: t('body.systemSetting.success_label'), detail: t('Notification.save_success'), life: 3000 });


    } catch (err) {
        toast.add({ severity: 'error', summary: t('Custom.error'), detail: t('Custom.errorOccurred'), life: 3000 });
        console.error("Lỗi khi gửi dữ liệu:", err)
    }
}

const confirmAddProduct = (products: any[]) => {
    products.forEach(newProduct => {
        const existingProductIndex = arrayProduct.value.findIndex(p => p.id === newProduct.id);
        if (existingProductIndex !== -1)
            arrayProduct.value[existingProductIndex].quantity = (arrayProduct.value[existingProductIndex].quantity || 1) + (newProduct.quantity || 1);
        else {
            newProduct.quantity = 1;
            arrayProduct.value.push(newProduct);
        }
    });
};
</script>
