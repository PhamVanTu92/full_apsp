<template>
    <div class="mx-3">
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.display_label')}}</div>
            <div class="col-8"><InputSwitch></InputSwitch></div>
            <div class="col-4">{{t('body.SaleSchannel.title_label')}}</div>
            <div class="col-8"><InputText class="w-full"></InputText></div>
            <div class="col-4">{{t('body.SaleSchannel.Menu')}}</div>
            <div class="col-8"><Dropdown placeholder="Chọn menu"></Dropdown></div>
        </div>
        <hr />
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.product_group_label')}}</div>
            <div class="col-8">
                <Dropdown :placeholder="t('body.SaleSchannel.select_product_group_placeholder')"></Dropdown>
            </div>
            <!-- <div class="col-4">Hiển thị đếm ngược</div>
            <div class="col-8 flex align-items-center gap-3">
                <div>
                    <InputSwitch></InputSwitch>
                    <label class="ml-2"></label>
                </div>
            </div> -->
            <!-- <div class="col-4">Đếm ngược</div>
            <div class="col-8 flex align-items-center gap-3">
                <Calendar v-model="x" class="w-11rem" showTime hourFormat="24"></Calendar>
                <span>Đến</span>
                <Calendar class="w-11rem"></Calendar>
            </div> -->
            <div class="col-4">{{t('body.SaleSchannel.number_of_products_to_display_label')}}</div>
            <div class="col-8">
                <InputNumber></InputNumber>
            </div>
        </div>
        <hr />
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.background_image_label')}}</div>
            <div class="col-8">
                <FilePicker
                    placeholder="Thêm ảnh"
                    :aspectRatio="8 / 16"
                    enableImageCropping
                ></FilePicker>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.content_label')}}</div>
            <div class="col-8">
                <Editor editorStyle="height: 220px" />
            </div>
            <div class="col-4">{{t('body.SaleSchannel.link_label')}}</div>
            <div class="col-8">
                <InputText class="w-full" />
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
