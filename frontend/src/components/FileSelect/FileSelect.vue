<template>
  <span>
    <InputGroup @click="onClickOpenFileSelect" v-if="props.showFileName">
      <InputText
        :value="placeholder"
        readonly
        class="cursor-pointer"
        :class="{ 'border-red-500': props.invalid }"
        :size="props.size"
      ></InputText>
      <Button
        :label="props.label"
        :outlined="props.outlined"
        :text="props.text"
        :icon="props.icon"
        :size="props.size"
        severity="secondary"
      />
    </InputGroup>
    <Button
      v-else
      @click="onClickOpenFileSelect"
      :label="props.label"
      :outlined="props.outlined"
      :text="props.text"
      :icon="props.icon"
      :size="props.size"
    />
    <input
      ref="fileInputRef"
      type="file"
      :multiple="props.multiple"
      :accept="props.accept"
      class="hidden"
      @change="onChangeFile"
    />
  </span>
</template>

<script setup lang="ts">
  import type { Booleanish } from "primevue/ts-helpers";
  import { ref, defineProps, defineEmits, withDefaults, onMounted } from "vue";
  import { useToast } from "primevue/usetoast";
  import { useI18n } from "vue-i18n";
  const { t } = useI18n();

  const toast = useToast();
  const fileInputRef = ref<HTMLElement>();
  const placeholder = ref("");

  interface Props {
    icon?: string;
    outlined?: boolean;
    text?: boolean;
    accept?: string;
    multiple?: Booleanish;
    label?: string;
    size?: "large" | "small";
    showFileName?: boolean;
    invalid?: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    accept: "*",
    label: "Chọn file",
  });

  const emits = defineEmits(["change"]);

  const validateFileType = (file: File, validTypes: string[]): boolean => {
    return validTypes.includes(file.type);
  };

  const onChangeFile = (e: Event) => {
    const target = e.target as HTMLInputElement;
    if (target?.files) {
      if (target.files.length > 1) {
        placeholder.value = target.files.length + " tập tin";
      } else {
        placeholder.value = target?.files[0].name;
      }
      if (props.accept == "*") {
        emits("change", target.files);
        return;
      }
      const validTypes = props.accept.split(",");
      const invalidFiles = Array.from(target.files).filter(
        (file) => !validateFileType(file, validTypes)
      );
      if (invalidFiles.length > 0) {
        toast.add({
          severity: "error",
          summary: t('common.error'),
          detail: t('common.msg_error_occurred'),
          life: 3000,
        });
        return;
      }

      emits("change", target.files);
    }
  };

  const onClickOpenFileSelect = () => {
    fileInputRef.value?.click();
  };

  onMounted(() => {
    // Any initialization logic if needed
  });
</script>
