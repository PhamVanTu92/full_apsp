<template>
    <button
        class="btn text-color-secondary border-dashed border-400 border-none bg-white hover:bg-green-50 hover:text-green-500 hover:border-green-500 border-bottom-1 px-2 py-3 cursor-pointer w-full flex justify-content-between"
        @click="visibleRight = true"
    >
        <div class="flex align-items-center justify-content-between w-full">
            <span class="text-base font-semibold block">
                <i class="pi pi-dollar"></i> {{t('client.promotion_for_you')}}</span
            >
            <Badge
                :value="
                    props.promotion?.promotionOrderLine?.length
                        ? props.promotion?.promotionOrderLine?.length
                        : 0
                "
                severity="success"
            ></Badge>
        </div>
    </button>
    <Sidebar
        v-model:visible="visibleRight"
        header="Danh sách khuyến mãi"
        position="right"
        class="w-full md:w-20rem lg:w-30rem"
    >
        <div class="card border-round-md bg-gray-100 flex flex-column gap-2">
            <div
                class="card bg-white border-round-md flex flex-column gap-1 relative p-0 py-3 px-5"
                v-for="item in props.promotion?.promotionOrderLine"
            >
                <span class="font-bold text-lg" for="">{{ item.promotionName }}</span>
                <span
                    class="mt-2 text-sm white-space-nowrap overflow-hidden text-overflow-ellipsis"
                >
                    {{ item.promotionDesc }}
                </span>
                <Button
                    label="Xem chi tiết"
                    @click="openDialog(item)"
                    class="mt-3"
                    outlined
                />
            </div>
        </div>
    </Sidebar>
    <Dialog
        v-model:visible="visible"
        modal
        :header="payloadDialog.name"
        :style="{ width: '40%' }"
    >
        {{ payloadDialog.description }}
        <div class="flex justify-content-end gap-2">
            <Button
                type="button"
                label="Đóng"
                icon="pi pi-sign-out"
                @click="visible = false"
            />
        </div>
    </Dialog>
</template>

<script setup>
import { ref, defineProps } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
    promotion: String,
});

const visibleRight = ref(false);
const visible = ref(false);
const payloadDialog = ref({
    name: "",
    description: "",
});

const openDialog = (data) => {
    visible.value = true;
    payloadDialog.value.name = data.promotionName;
    payloadDialog.value.description = data.promotionDesc;

};
</script>
