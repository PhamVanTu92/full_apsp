<template>
    <Dropdown v-model="selectedCountry" class="bg-white-alpha-10 border-none" :options="countries" placeholder=""
        pt:trigger:class="hidden" pt:input:class="p-1"
        @update:modelValue="ChangeLanguage">
        <template #value="slotProps">
            <div v-if="slotProps.value" class="flex align-items-center">
                <img :alt="slotProps.value.label"
                    src="https://primefaces.org/cdn/primevue/images/flag/flag_placeholder.png"
                    :class="`flag flag-${slotProps.value.code?.toLowerCase()}`" style="width: 28px" />
            </div>
            <span v-else>
                {{ slotProps.placeholder }}
            </span>
        </template>
        <template #option="slotProps">
            <div class="flex align-items-center">
                <img :alt="slotProps.option.label"
                    src="https://primefaces.org/cdn/primevue/images/flag/flag_placeholder.png"
                    :class="`mr-2 flag flag-${slotProps.option.code?.toLowerCase()}`" style="width: 18px" />
                <div>{{ slotProps.option.name }}</div>
            </div>
        </template>
    </Dropdown>
</template>

<script setup>
import { ref } from "vue";

import { useI18n } from 'vue-i18n';

const { locale } = useI18n();
const selectedCountry = ref(locale.value === 'vi' ? { name: 'Tiếng Việt', code: 'VN' } : { name: "English", code: "US" });
const countries = ref([
    { name: 'Tiếng Việt', code: 'VN' },
    { name: "English", code: "US" },
]);

const ChangeLanguage = (e) => {
    if (e.code === 'US') {
        locale.value = 'en';
        localStorage.setItem('language-pos', 'en');
    } else if (e.code === 'VN') {
        locale.value = 'vi';
        localStorage.setItem('language-pos', 'vi');
    }
};
</script>
