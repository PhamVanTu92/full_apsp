<template>
    <div>
        <InputGroup @click="onClickOpenFile">
            <InputText
                :disabled="props.disabled"
                :placeholder="props.placeholder"
                readonly
                :value="fileName"
                class="cursor-pointer"
            ></InputText>
            <Button icon="pi pi-file" severity="secondary"/>
        </InputGroup>
        <input
            @input="onChange"
            ref="inputFile"
            type="file"
            class="hidden"
            :multiple="props.multiple"
        />

        <Dialog v-model:visible="visibleCropper" :header="t('common.btn_edit')" class="w-5" modal>
            <VuePictureCropper
                :boxStyle="boxStyle"
                :img="pic"
                :options="cropOption"
                @ready="ready"
            />
            <!-- <template v-if="result.base64">
                <img :src="result.base64" alt="" />
            </template> -->
            <template #footer>
                <div class="flex justify-content-between gap-2 flex-grow-1">
                    <div class="flex gap-2">
                        <Button @click="cropper?.rotate(-90)" icon="fa-solid fa-rotate-left"/>                        
                        <Button @click="cropper?.rotate(90)" icon="fa-solid fa-rotate-right"/>                        
                    </div>
                    <div class="flex gap-2">
                        <Button :label="t('body.status.HUY2')" @click="visibleCropper = false" severity="secondary" />
                        <Button :label="t('body.status.XN')" @click="onConfirmCrop" />
                    </div>
                </div>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import VuePictureCropper, { cropper } from "vue-picture-cropper";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const inputFile = ref<HTMLElement>();
const fileName = ref();

export interface Props {
    disabled?: boolean | undefined;
    multiple?: boolean | undefined;
    placeholder?: string | undefined;
    enableImageCropping?: boolean | undefined;
    aspectRatio?: number | undefined
}
const emits = defineEmits(["cropped"]);
const props = withDefaults(defineProps<Props>(), {
    disabled: false,
    multiple: false,
    placeholder: "Chọn tài liệu",
    pictureCrop: false,
    aspectRatio: 1
});
const pic = ref();
const visibleCropper = ref(false);

// const result = reactive({
//     base64: "",
//     file: null as File | null,
//     blob: null as null | Blob,
// });
const onConfirmCrop = async () => {
    // Perform your cropping logic here
    if (!cropper) return;
    cropper?.crop();
    const base64 = cropper.getDataURL();
    const blob = await cropper.getBlob();
    if (!blob) return;
    const file = await cropper.getFile({
        fileName: fileName.value,
    });
    emits("cropped", {
        base64,
        file,
        blob,
    });
    visibleCropper.value = false;
};

const ready = (event) => {

};

const onChange = (e: Event): void => {
    const target = e.target as HTMLInputElement;
    const file: File | null = target.files?.[0] || null;
    fileName.value = file?.name;
    if ((props.enableImageCropping || props.enableImageCropping === undefined ) && file) {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => {
            // Update the picture source of the `img` prop
            pic.value = String(reader.result);

            // Show the modal
            visibleCropper.value = true;

            // Clear selected files of input element
            // if (!uploadInput.value) return
            // uploadInput.value.value = ''
        };
    }
};

const onClickOpenFile = (): void => {
    inputFile.value?.click();
};

const boxStyle = {
    width: "100%",
    height: "100%",
    backgroundColor: "#f8f8f8",
    margin: "auto",
};
const cropOption = {
    viewMode: 1,
    dragMode: "crop",
    aspectRatio: props.aspectRatio,
};

onMounted(function () {});
</script>

<style scoped></style>
