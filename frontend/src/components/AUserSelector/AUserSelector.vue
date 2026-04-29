<template>
    <span>
        <AutoComplete
            v-model="selection"
            :suggestions="suggestions"
            optionLabel="fullName"
            id="selectUser"
            :completeOnFocus="props.completeOnFocus"
            :loading="loading"
            pt:panel:id="scrollUser"
            :placeholder="props.placeholder"
            :disabled="props.disabled"
            @complete="onSearch"
            @focus="onForcus"
            @item-select="onSelect"
            @item-unselect="onUnselect"
        >
            <template #content> dsa </template>
            <template #option="slotProps">
                <div class="flex gap-2 align-items-center">
                    <div
                        class="surface-200 flex align-items-center justify-content-center border-round"
                        style="width: 30px; height: 30px"
                    >
                        <i class="pi pi-user"></i>
                    </div>
                    <div>{{ slotProps.option.fullName }}</div>
                </div>
            </template>
        </AutoComplete>
    </span>
</template>

<script setup lang="ts">
import { ref, reactive, nextTick, computed, watch, onMounted } from "vue";
import API from "../../api/api-main";
import { AutoCompleteCompleteEvent } from "primevue/autocomplete";
import { useInfiniteScroll } from "@vueuse/core";

const props = withDefaults(
    defineProps<{
        placeholder?: string;
        userType?: "APSP" | "NPP";
        optionValue?: string;
        completeOnFocus?: boolean;
        disabled?: boolean;
    }>(),
    {
        userType: "APSP",
    }
);
const modelValue = defineModel();
const PATH = "account/getall";
const suggestions = ref<any[]>([]);
const selection = ref();
const scrollElement = ref();
const loading = ref(false);
var isEmpty = false;
const queryParams = reactive({
    search: "",
    skip: 0,
    limit: 10,
    userType: props.userType,
});

const onSelect = () => {
    if (props.optionValue) {
        modelValue.value = selection.value[props.optionValue];
    } else {
        modelValue.value = selection.value;
    }
};

const onUnselect = () => {
    modelValue.value = null;
};

const onForcus = () => {
    setTimeout(() => {
        scrollElement.value = document.querySelector("#scrollUser");
        loading.value = false;
        isEmpty = false;
    }, 100);
};

useInfiniteScroll(
    scrollElement,
    () => { 
        if (!loading.value && !isEmpty) {
            queryParams.skip += 1;
            fetchUser(true);
        }
    },
    { distance: 100 }
);

const getQueryString = (): string => {
    return Object.entries(queryParams)
        .filter(([_, value]) => value !== undefined && value !== null && value !== "")
        .map(([key, value]) => `${key}=${value}`)
        .join("&")
        .replace(/^/, "?");
};

const onSearch = (e: AutoCompleteCompleteEvent) => {
    queryParams.search = e.query;
    queryParams.skip = 0;
    onForcus();
    fetchUser();
};

const fetchUser = (isAppend = false) => {
    loading.value = true;
    API.get(`${PATH}${getQueryString()}`)
        .then((res) => {
            if (res.data.item?.length < 1) {
                isEmpty = true;
            }
            if (isAppend) {
                suggestions.value = [...suggestions.value, ...res.data.item];
            } else {
                suggestions.value = res.data.item;
            } 
        })
        .catch()
        .finally(() => {
            loading.value = false;
        });
};

const initialComponent = () => {
    // code here
    fetchUser();
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
