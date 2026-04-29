<template>
    <div class="">
        <div class="field">
            <label>{{ t('client.customer_note') }}</label>
            <p class="note-input">{{ odStore?.note }}</p>
        </div>
        <div class="field">
            <label>{{ t('client.apsp_note') }}</label>
            <Textarea v-model="note" v-if="editMode" class="w-full" auto-resize :rows="3" id="sellerNote"
                @keydown.esc="onClickCancel"></Textarea>
            <p v-else class="note-input">{{ odStore?.sellerNote }}</p>
        </div> 
        <div class="flex justify-content-end gap-2" v-if="odStore?.status !== 'DHT' && !props.isClient">
            <template v-if="editMode">
                <Button severity="secondary" :label="t('body.status.HUY2')" @click="onClickCancel" />
                <Button :loading="loadingNote" :label="t('client.save')" @click="onClickSaveNote" />
            </template>
            <Button @click="onClickEdit" v-else :label="t('client.edit')" />
        </div>
    </div>
</template>

<script setup lang="ts">
import API from "@/api/api-main";
import { ref,  onMounted } from "vue";  
import { nextTick } from "vue";
import { useToast } from "primevue/usetoast";
import { useI18n } from 'vue-i18n';
const { t } = useI18n(); 
const toast = useToast();

const props = defineProps({
    isClient: {
        default: false,
        required: false,
        type: Boolean,
    },
    odStore : {
        default: null,
        required: true,
        type: Object,
    }
});

const initialComponent = () => {
    // code here
};

const note = ref("");
const editMode = ref(false);

const loadingNote = ref(false);
const onClickSaveNote = async () => {
    loadingNote.value = true;
    try {
        let body = { sellerNote: note.value?.trim() };
        await API.patch(`PurchaseRequest/${props.odStore?.id}`, body as any);
        // if (props.odStore?.id) props.odStore.fetchStore(props.odStore?.id);  
        onClickCancel();
    } catch (error) {
        toast.add({
            severity: "error",
            summary: "Lỗi",
            detail: t('Notification.update_error_note'),
            life: 3000,
        });
    } finally {
        loadingNote.value = false;
    }
};

const onClickCancel = () => {
    note.value = "";
    editMode.value = false;
};
const onClickEdit = () => {
    editMode.value = true;
    note.value = props.odStore?.sellerNote || "";
    nextTick(() => {
        document.getElementById("sellerNote")?.focus();
    });
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.note-input {
    padding: 1rem;
    border-left: 3px solid var(--blue-200);
    background-color: var(--surface-100);
    min-height: 49px;
}

label {
    font-weight: bold;
}
</style>
