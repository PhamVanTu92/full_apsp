<template>
    <div class="flex flex-column gap-3">
        <template v-for="(p, i) in promotions" :key="i">
            <div class="border-1 border-200 p-3 surface-100">
                <div class="text-lg text-primary mb-2 font-bold">
                    {{ p.name }}
                </div>
                <p class="font-normal">
                    {{ p.description }}
                </p>
                <DataTable
                    v-if="p.data.filter((item) => item.quantityAdd > 0).length"
                    :value="p.data.filter((item) => item.quantityAdd > 0)"
                    show-gridlines
                    scrollable
                    scroll-height="235px"
                >
                    <Column header="#" class="w-3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column field="itemName" :header="t('client.productName')"></Column>
                    <Column
                        field="packingName"
                        :header="t('client.unit')"
                        class="w-10rem"
                    ></Column>
                    <Column :header="t('client.quantity')" class="w-7rem text-right">
                        <template #body="{ data }">
                            {{ fnum(data["quantityAdd"]) }}
                        </template>
                    </Column>
                    <Column :header="t('client.line')" class="w-7rem text-right">
                        <template #body="{ data }">
                            {{ data["lineId"] + 1 }}
                        </template>
                    </Column>
                </DataTable>
            </div>
        </template>
        <!-- {{ odStore.order?.promotion }} -->
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import ls from "lodash";

import { useOrderDetailStore } from "../store/orderDetail";
import { fnum } from "../../PurchaseOrder/script";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const odStore = useOrderDetailStore();
type PromotionShow = {
    code: string;
    data: any[];
    description: string;
    name: string;
};

const promotions = computed(() => {
    const prms = odStore.order?.promotion;
    let result: PromotionShow[] = [];
    if (ls.isArray(prms)) {
        const prmGrouped = ls(prms)
            .groupBy("promotionCode")
            .map((items, code) => {
                return {
                    code: items[0].promotionCode,
                    name: items[0].promotionName,
                    description: items[0].promotionDesc,
                    data: items,
                };
            })
            .value();
        result = prmGrouped;
    }
    return result;
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
