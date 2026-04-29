<template>
    <div class="mx-3">
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.display_label')}}</div>
            <div class="col-8"><InputSwitch></InputSwitch></div>
            <div class="col-4">{{t('body.SaleSchannel.title_label')}}</div>
            <div class="col-8"><InputText class="w-full"></InputText></div>
        </div>
        <hr />
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.product_group_label')}}</div>
            <div class="col-8">
                <Dropdown placeholder="Chọn nhóm sản phẩm hiển thị"></Dropdown>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.display_countdown_label')}}</div>
            <div class="col-8 flex align-items-center gap-3">
                <div>
                    <InputSwitch></InputSwitch>
                    <label class="ml-2"></label>
                </div>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.countdown_label')}}</div>
            <div class="col-8 flex align-items-center gap-3">
                <Calendar v-model="x" class="w-11rem" showTime hourFormat="24"></Calendar>
                <span>{{t('body.SaleSchannel.to_label')}}</span>
                <Calendar class="w-11rem"></Calendar>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.number_of_products_to_display_label')}}</div>
            <div class="col-8">
                <InputNumber></InputNumber>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const x = ref();

onMounted(function () {});
</script>

<style scoped>
.grid > div {
    padding-bottom: 0;
}
</style>
