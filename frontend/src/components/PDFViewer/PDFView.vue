<template>
    <div>
        <Dialog :header="props.header || '  '" :class="props.class" :style="props.style" v-model:visible="visible" modal content-style="min-height: 700px" style="min-width: 50rem">
            <div v-for="page in pages" :key="page">
                <VuePDF :pdf="pdf" :page="page" />
            </div>
            <template #footer>
                <Button :label="t('body.OrderList.close')" @click="visible = false" severity="secondary"/>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { VuePDF, usePDF } from '@tato30/vue-pdf';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const props = defineProps({
    header: {
        type: String,
        required: false
    },
    url: {
        type: String,
        required: true
    },
    class: {
        type: String,
        required: false
    },
    style: {
        type: String,
        required: false
    }
});

const visible = defineModel('visible', {
    default: false
});

const { pdf, pages } = usePDF({
    url: props.url
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
