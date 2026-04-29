<template>
    <MultiSelect
        :class="props.class"
        selectedItemsLabel="{0} lựa chọn"
        :maxSelectedLabels="props.maxSelectedLabels"
        v-model="modelValue"
        :options="props.options"
        :optionLabel="props.optionLabel"
        :optionValue="props.optionValue"
        :placeholder="props.placeholder"
        filter
        :disabled="props.disabled"
        resetFilterOnHide
        selectOnFocus
        highlightOnSelect
        :selectAll="isSelectAll"
        @selectall-change="onSelectAll"
        @filter="onFilter"
        @change="onChange"
        @before-hide="onBeforeHide"
        @before-show="onBeforeShow"
    >
        <template #emptyfilter>
            <div class="py-5 my-5 text-center">Không có dữ liệu</div>
        </template>
    </MultiSelect>
</template>

<script setup>
import { ref, watch } from "vue";
const modelValue = defineModel({
    type: [Number],
});
const emits = defineEmits(["change", "update:modelValue"]);
const props = defineProps({
    options: {
        type: Array,
        default: [],
        required: true,
    },
    maxSelectedLabels: {
        type: Number,
        default: 3,
    },
    optionLabel: {
        type: String,
        required: true,
    },
    optionValue: {
        type: String,
        required: true,
    },
    class: {
        type: String,
        default: "w-2",
    },
    placeholder: {
        type: String,
    },
    disabled: {
        default: false,
    },
});

const filterKey = ref("");
const isSelectAll = ref(false);
const onSelectAll = (e) => {
    const ids = getFiltedData();
    if (ids.length > 0) {
        isSelectAll.value = !isSelectAll.value;
    }
    if (isSelectAll.value) {
        ids.forEach((id) => {
            if (!modelValue.value.includes(id)) {
                modelValue.value.push(id);
            }
        });
    } else {
        ids.forEach((id) => {
            const idx = modelValue.value.findIndex((el) => el == id);
            if (idx != -1) modelValue.value.splice(idx, 1);
        });
    }
    // emits("change", modelValue.value);
};
var timer = null;
const onFilter = (e) => {
    filterKey.value = e.value;
    if (timer) {
        clearTimeout(timer);
    }
    timer = setTimeout(() => {
        const filtedData = getFiltedData();
        let counter = 0;
        filtedData.forEach((el) => {
            if (modelValue.value.includes(el)) {
                counter++;
            }
        });
        if (filtedData.length < 1) {
            isSelectAll.value = false;
            return;
        }
        if (counter == filtedData.length) {
            isSelectAll.value = true;
            return;
        }
    }, 10);
    if (modelValue.value.length == props.options.length) {
        isSelectAll.value = true;
    } else {
        isSelectAll.value = false;
    }
};

const onChange = (e) => {
    const filtedIds = getFiltedData();
    isSelectAll.value = compareIdArray(e.value, filtedIds);
};

const getFiltedData = () => {
    if (!filterKey.value) {
        return props.options.map((el) => el[props.optionValue]);
    }
    const filtedData = props.options
        .filter((el) =>
            el[props.optionLabel]?.toLowerCase().includes(filterKey.value.toLowerCase())
        )
        .map((el) => el[props.optionValue]);
    return filtedData || [];
};

watch(
    () => modelValue.value.length,
    () => {
        if (modelValue.value.length < 1) {
            isSelectAll.value = false;
        }
        emits("change", modelValue.value);
    }
);

const onBeforeHide = () => {
    filterKey.value = "";
};
const onBeforeShow = () => {
    isSelectAll.value = modelValue.value.length == getFiltedData().length;
};

function compareIdArray(src, target) {
    for (let i = 0; i < target.length; i++) {
        if (!src.includes(target[i])) {
            return false;
        }
    }
    return true;
}
</script>

<style></style>
