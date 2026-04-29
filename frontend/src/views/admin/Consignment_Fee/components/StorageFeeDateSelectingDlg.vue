<template>
    <Dialog v-model:visible="visible" modal :header="t('body.sampleRequest.warehouseFee.dialog_title')" :style="{ width: '25rem' }">
        <div class="flex align-items-center gap-3 pb-3">
            <label class="font-bold w-5rem">{{ t('body.sampleRequest.warehouseFee.year_label') }}</label>
            <Calendar v-model="selectedYear" view="year" dateFormat="yy" :placeholder="t('body.sampleRequest.warehouseFee.year_placeholder')" class="w-full" />
        </div>
        <div class="flex align-items-center gap-3 pb-5">
            <label class="font-bold w-5rem">{{ t('body.sampleRequest.warehouseFee.quarter_label') }}</label>
            <Dropdown v-model="selectedQuarter" :options="quarterList" optionValue="value" optionLabel="label" :placeholder="t('body.sampleRequest.warehouseFee.quarter_placeholder')" class="w-full" />
        </div>
        <div class="flex justify-content-end gap-2">
            <Button type="button" :label="t('body.sampleRequest.importPlan.cancel_button')" @click="onCancelClick" severity="secondary"/>
            <Button type="button" :label="t('body.systemSetting.save_button')" @click="onSaveClick"/>
        </div>
    </Dialog>
</template>

<script setup>
// Imports
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

// Emit
const emits = defineEmits(['yearQuarterSaved', 'yearQuarterCanceled']);

// Props

// Models
const visible = defineModel('visible', {
    type: Boolean,
    default: false
});

const selectedYear = defineModel('selectedYear', {
    type: [String, Date],
    default: new Date()
});

const selectedQuarter = defineModel('selectedQuarter', {
    type: String,
    default: ''
});

// Internal States
const rememberedYear = selectedYear.value;
const rememberedQuarter = selectedQuarter.value;

// I18n
const { t } = useI18n();

// Computed States
const quarterList = computed(() => {
    let resultList = [];
    if ((!selectedYear.value)) {
        return resultList; // Nếu năm rỗng thì không trả về quý rỗng
    }
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth();
    const numericSearchYear = editSelectedYear(selectedYear.value);

    // Determine the number of quarters that have passed
    let listLength = numericSearchYear < currentYear ? 4 : numericSearchYear === currentYear ? (currentMonth >= 9 ? 4 : currentMonth >= 6 ? 3 : currentMonth >= 3 ? 2 : 1) : 0; // For future years, no quarters passed

    // set danh sách quý
    for (let index = 0; index < listLength; index++) {
        resultList.push({
            value: `${index + 1}`,
            label: t('body.sampleRequest.warehouseFee.quarter_label') +` ${index + 1}`
        });
    }
    return resultList;
});

// Event Funtions

const onSaveClick = () => {
    const payload = { selectedYear: selectedYear.value, selectedQuarter: selectedQuarter.value};
    emits('yearQuarterSaved', payload);
};

const onCancelClick = () => {
    selectedYear.value = rememberedYear;
    selectedQuarter.value = rememberedQuarter;
    emits('yearQuarterCanceled');
};

// Function
const editSelectedYear = (year) => {
    if (!year) return 0; //searchYear trên màn hình nhập rỗng thì trả về null nếu chuyển Date sẽ cho ra năm 1970 NG -> chuyển thành 0 OK
    const date = new Date(year); //searchYear ở đây là 1 Date string literal -> chuyển về Date để lấy năm
    return isNaN(date.getFullYear()) ? 0 : date.getFullYear(); //giữ là số để so sánh, khi gán vào payload sẽ tự động chuyển thành string
};
</script>

<style scope></style>
