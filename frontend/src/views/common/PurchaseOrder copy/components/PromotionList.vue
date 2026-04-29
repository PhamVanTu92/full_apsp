<template>
    <!-- :loading="poStore.loadingOrderButton" -->
    <DataTable
        v-if="poStore.getPromtionToShow.length"
        v-model:expandedRows="expandedRows"
        :value="poStore.getPromtionToShow"
        :showGridlines="true"
        class="mb-3"
    >
        <template #header>
            <div class="text-lg">Khuyến mãi</div>
        </template>
        <Column
            v-if="
                poStore.getPromtionToShow?.some(
                    (item) =>
                        Object.keys(item._choiceGroup).length || item._giftGroup?.length
                )
            "
            expander
            class="w-4rem"
        >
        </Column>
        <Column field="promotionName" header="Chương trình khuyến mại"></Column>
        <Column field="promotionDesc" header="Mô tả"></Column>
        <template #expansion="slotProps">
            <template
                v-if="
                    Object.keys(slotProps.data._choiceGroup).length > 0 ||
                    slotProps.data._giftGroup.length > 0
                "
            >
                <!-- Header -->
                <DataTable :pt="{ tbody: { class: 'hidden' } }">
                    <template #header>
                        <div>Danh sách vật phẩm tặng</div>
                    </template>
                    <Column class="w-selection" header=""></Column>
                    <Column class="" header="Tên sản phẩm"></Column>
                    <Column class="w-7rem" header="Số lượng"></Column>
                    <Column class="w-15rem" header="Đơn vị tính"></Column>
                    <Column class="w-15rem" header="Loại sản phẩm"></Column>
                    <Column class="w-5rem" header="Dòng"></Column>
                </DataTable>
                <!-- Header -->
                <!-- Sản phẩm chọn -->
                <DataTable
                    v-if="Object.keys(slotProps.data._choiceGroup).length > 0"
                    v-model:selection="slotProps.data._selection"
                    :value="Object.keys(slotProps.data._choiceGroup)"
                    selectionMode="single"
                    :metaKeySelection="true"
                    :pt="{ headerRow: { class: 'hidden' } }"
                    @row-select="onChangeCombo"
                >
                    <Column
                        field=""
                        class="w-selection"
                        header=""
                        selectionMode="single"
                    ></Column>
                    <Column field="itemName" class="" header="Tên sản phẩm">
                        <template #body="{ data, field }">
                            <div class="collapse-row">
                                <div
                                    v-for="(item, idx) in slotProps.data._promotionItems[
                                        data
                                    ]"
                                    :key="idx"
                                    class="flex"
                                >
                                    <div>
                                        {{ item[field] }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Column>
                    <Column field="quantityAdd" class="w-7rem" header="Số lượng">
                        <template #body="{ data, field }">
                            <div class="collapse-row">
                                <div
                                    v-for="(item, idx) in slotProps.data._promotionItems[
                                        data
                                    ]"
                                    :key="idx"
                                    class="text-right"
                                >
                                    <div>
                                        {{ item[field] }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Column>
                    <Column field="packingName" class="w-15rem" header="Đơn vị tính">
                        <template #body="{ data, field }">
                            <div class="collapse-row">
                                <div
                                    v-for="(item, idx) in slotProps.data._promotionItems[
                                        data
                                    ]"
                                    :key="idx"
                                >
                                    <div>
                                        {{ item[field] }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Column>
                    <Column field="itemGroup" class="w-15rem" header="Loại sản phẩm">
                        <template #body="{ data, field }">
                            <div class="collapse-row">
                                <div
                                    v-for="(item, idx) in slotProps.data._promotionItems[
                                        data
                                    ]"
                                    :key="idx"
                                >
                                    <div>
                                        {{ getItemGroupLabel(item[field]) }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Column>
                    <Column field="lineId" class="w-5rem" header="Dòng">
                        <template #body="{ data, field }">
                            <div class="collapse-row">
                                <div
                                    v-for="(item, idx) in slotProps.data._promotionItems[
                                        data
                                    ]"
                                    :key="idx"
                                    class="text-right"
                                >
                                    <div>
                                        {{ item[field] + 1 }}
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Column>
                </DataTable>
                <!-- Sản phẩm chọn -->
                <!-- Sản phẩm tặng -->
                <DataTable
                    v-if="slotProps.data._giftGroup.length > 0"
                    :value="slotProps.data._giftGroup"
                    :pt="{ headerRow: { class: 'hidden' } }"
                >
                    <Column field="" class="w-selection" header="">
                        <template #body>
                            <div
                                class="border-round-sm flex align-items-center justify-content-center surface-300"
                                style="width: 17.5px; height: 17.5px; padding: 1px"
                            >
                                <i class="pi pi-check" style="font-size: x-small"></i>
                            </div>
                        </template>
                    </Column>
                    <Column field="itemName" class="" header="Tên sản phẩm"></Column>
                    <Column
                        field="quantityAdd"
                        class="w-7rem text-right"
                        header="Số lượng"
                    ></Column>
                    <Column
                        field="packingName"
                        class="w-15rem"
                        header="Đơn vị tính"
                    ></Column>
                    <Column field="itemGroup" class="w-15rem" header="Loại sản phẩm">
                        <template #body="{ data, field }">
                            {{ getItemGroupLabel(data[field]) }}
                        </template>
                    </Column>
                    <Column field="lineId" class="w-5rem text-right" header="Dòng">
                        <template #body="{ data, field }">
                            {{ data[field] + 1 }}
                        </template>
                    </Column>
                </DataTable>
                <!-- Sản phẩm tặng -->
            </template>
        </template>
    </DataTable>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, nextTick } from "vue";
import { usePoStore } from "../store/purchaseStore.store";
import { debounce } from "lodash";
const poStore = usePoStore();
const expandedRows = ref<any>([]);

const onChangeCombo = () => {
    poStore.fetchCheckPriceMethod();
};

const dbcer = debounce(() => {

    expandedRows.value = poStore.getPromtionToShow.filter((item) => {
        return Object.keys(item._choiceGroup).length > 0 || item._giftGroup?.length;
    });
}, 10);

watch(
    () => JSON.stringify(poStore.getPromtionToShow),
    () => {
        // const expan = poStore.promotionToShow.some((item) => {
        //     return Object.keys(item._choiceGroup).length > 0 || item._giftGroup?.length;
        // });
        // if (expan) {
        //     expandedRows.value = poStore.promotionToShow;
        // }
        expandedRows.value = [];
        dbcer();
    }
);

const getItemGroupLabel = (key: string) => {
    return key === "KH" ? "Hàng khuyến mại" : "Vật phẩm khuyến mại";
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style>
.collapse-row > *:not(:last-child) {
    padding-bottom: 0.75rem;
    border-bottom: 1px solid var(--surface-border);
    margin-bottom: 0.75rem;
}
.w-selection {
    width: 3.5rem !important;
}
.no-border-top * {
    border-top: none !important;
    background: none;
}
.no-border * {
    border: none !important;
}
</style>
