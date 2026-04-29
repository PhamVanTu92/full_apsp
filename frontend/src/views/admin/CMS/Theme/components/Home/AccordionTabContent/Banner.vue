<template>
    <div class="mx-3">
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.left_banner_label')}}</div>
            <div class="col-8"><InputSwitch></InputSwitch></div>
            <div class="col-4">{{t('body.SaleSchannel.content_label')}}</div>
            <div class="col-8">
                <Editor class="w-full" editorStyle="height: 220px"></Editor>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.link_label')}}</div>
            <div class="col-8"><InputText class="w-full"></InputText></div>
            <div class="col-4">{{t('body.SaleSchannel.background_image_label')}}</div>
            <div class="col-8">
                <FilePicker
                    placeholder="Thêm banner"
                    :aspectRatio="2.66"
                    enableImageCropping
                ></FilePicker>
            </div>
        </div>
        <hr />
        <div class="grid align-items-center">
            <div class="col-4">{{t('body.SaleSchannel.right_banner_label')}}</div>
            <div class="col-8"><InputSwitch></InputSwitch></div>
            <div class="col-4">{{t('body.SaleSchannel.content_label')}}</div>
            <div class="col-8">
                <Editor class="w-full" editorStyle="height: 220px"></Editor>
            </div>
            <div class="col-4">{{t('body.SaleSchannel.link_label')}}</div>
            <div class="col-8"><InputText class="w-full"></InputText></div>
            <div class="col-4">{{t('body.SaleSchannel.background_image_label')}}</div>
            <div class="col-8">
                <FilePicker
                    placeholder="Thêm banner"
                    :aspectRatio="2.66"
                    enableImageCropping
                ></FilePicker>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

onMounted(function () {});
</script>

<style scoped>
.grid > div {
    padding-bottom: 0px;
}
</style>
