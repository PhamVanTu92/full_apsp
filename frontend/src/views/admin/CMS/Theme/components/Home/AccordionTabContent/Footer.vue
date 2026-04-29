<template>
    <div>
        <div class="grid">
            <div class="col-3">{{t('body.SaleSchannel.hotline_label')}}</div>
            <div class="col-9"><InputText class="w-full"></InputText></div>
            <div class="col-3">{{t('body.SaleSchannel.address_label')}}</div>
            <div class="col-9"><InputText class="w-full"></InputText></div>
            <div class="col-3">{{t('body.SaleSchannel.email_label')}} </div>
            <div class="col-9"><InputText class="w-full"></InputText></div>
        </div>
        <hr>
        <div class="grid">
            <div class="col-3">Menu 1</div>
            <div class="col-9"><Dropdown class="w-full"></Dropdown></div>
            <div class="col-3">Menu 2</div>
            <div class="col-9"><Dropdown class="w-full"></Dropdown></div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.grid {
    align-items: center;
}
.grid > div {
    padding-bottom: 0;
}
</style>
