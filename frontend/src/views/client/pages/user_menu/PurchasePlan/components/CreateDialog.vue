<template>
    <Dialog
        @hide="onClose"
        v-model:visible="visible"
        :header="props.header"
        modal
        class="w-10"
    >
        <div class="grid m-0 mt-0">
            <div class="grid col-9">
                <div class="flex flex-column field col-3 py-0">
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.plan_id_label') }}</label>
                    <InputText :placeholder="t('body.sampleRequest.importPlan.plan_id_value')" disabled></InputText>
                </div>
                <div class="flex flex-column field col-9 py-0">
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.plan_name_label') }} <span class="text-red-500">*</span></label>
                    <IconField>
                        <InputIcon
                            v-if="enableGenrate"
                            class="fa-solid fa-wand-magic-sparkles"
                            v-tooltip="t('body.sampleRequest.importPlan.plan_name_placeholder')" 
                        >
                        </InputIcon>
                        <InputText
                            @keyup="checkEnable"
                            v-model="modelSates.planName"
                            :placeholder="t('body.sampleRequest.importPlan.plan_name_placeholder')"
                            class="w-full"
                            :invalid="submited && !modelSates.planName"
                        ></InputText>
                    </IconField>

                    <small v-if="submited && !modelSates.planName" class="text-red-500">{{ t('body.promotion.validation_name_required') }}</small>
                </div>
                <div
                    class="flex flex-column field col-2 py-0"
                    :class="{
                        'col-4': authStore.userType !== 'APSP',
                    }"
                >
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.start_date_label') }} <span class="text-red-500">*</span></label>
                    <Calendar
                        @date-select="onChangeFromDate"
                        v-model="modelSates.startDate"
                        :minDate="minFromDate"
                        dateFormat="dd/mm/yy"
                    ></Calendar>
                    <small v-if="!!errMsg.startDate" class="text-red-500">{{ errMsg.startDate }}</small>
                </div>
                <div
                    class="flex flex-column field col-2 py-0"
                    :class="{
                        'col-4': authStore.userType !== 'APSP',
                    }"
                >
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.end_date_label') }} <span class="text-red-500">*</span></label>
                    <Calendar
                        @date-select="onChangeEndDate"
                        :minDate="minToDate"
                        v-model="modelSates.endDate"
                        dateFormat="dd/mm/yy"
                        :invalid="!!errMsg.endDate"
                    ></Calendar>
                    <small v-if="!!errMsg.endDate" class="text-red-500">{{ errMsg.endDate }}</small>
                </div>
                <div
                    class="flex flex-column field col-2 py-0"
                    :class="{
                        'col-4': authStore.userType !== 'APSP',
                    }"
                >
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.plan_by_label') }}</label>
                    <Dropdown
                        @change="onChangeFomat"
                        v-model="modelSates.periodType"
                        :options="optionsDataColumns"
                        optionLabel="label"
                        optionValue="value"
                    ></Dropdown>
                </div>
                <div
                    class="flex flex-column field col-6 py-0"
                    v-if="authStore.userType === 'APSP'"
                >
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.customer_label') }} <span class="text-red-500">*</span></label>
                    <InputGroup>
                        <AutoComplete
                            :pt:input:class="'w-30rem'"
                            :invalid="errMsg.customer"
                            v-model="customer"
                            :suggestions="DataCustomer"
                            optionLabel="cardName"
                            @complete="GetCustomer(customer)"
                        >
                            <template #option="data">
                                <div class="flex gap-2 align-items-center">
                                    <Avatar
                                        icon="pi pi-user"
                                        class="mr-2"
                                        size="large"
                                        style="background-color: #ece9fc; color: #2a1261"
                                        shape="circle"
                                    />
                                    <div class="flex flex-column gap-1">
                                        <span class="font-bold">{{ data.option.cardName }}</span>
                                        <span class="text-gray-500">{{ data.option.email }}</span>
                                    </div>
                                </div>
                            </template>
                        </AutoComplete>
                        <InputGroupAddon :class="{ 'border-red-400': !!errMsg.customer }">
                            <i class="pi pi-search"></i>
                        </InputGroupAddon>
                    </InputGroup>
                    <small v-if="!!errMsg.customer" class="text-red-500">{{ errMsg.customer }}</small>
                </div>
            </div>
            <div class="grid col">
                <div class="flex flex-column field col py-0">
                    <label class="font-semibold" for="">{{ t('body.sampleRequest.importPlan.notes_label') }}</label>
                    <Textarea
                        v-model="modelSates.notes"
                        rows="3"
                        class="w-full h-full"
                        :placeholder="t('body.sampleRequest.importPlan.notes_placeholder')"
                    ></Textarea>
                </div>
            </div>
        </div>
        <TabView class="card p-0 overflow-hidden" value="0">
            <TabPanel :header="t('body.sampleRequest.importPlan.product_tab')">
                <small v-if="submited && !hasValidQuantity" class="text-red-500">{{ t('body.sampleRequest.importPlan.empty_state_message') }}</small>

                <DataTable :value="modelSates.saleForecastItems" scrollable showGridlines>
                    <Column header="#" style="min-width: 5rem">
                        <template #body="sp">
                            {{ sp.index + 1 }}
                        </template>
                    </Column>
                    <Column
                        field="itemName"
                        :header="t('body.sampleRequest.importPlan.table_header_product_name')"
                        style="min-width: 30rem"
                    ></Column>
                    <Column :header="t('body.sampleRequest.importPlan.table_header_unit')" style="min-width: 10rem">
                        <template #body="sp">
                            {{ sp.data.packing?.name }}
                        </template>
                    </Column>
                    <Column
                        v-for="(col, i) in columns"
                        :key="i"
                        :header="col"
                        style="min-width: 7rem"
                    >
                        <template #body="sp">
                            <InputNumber
                                v-model="sp.data.periods[i].quantity"
                                :pt:input:root:class="'w-7rem text-right'"
                                placeholder="0"
                                :min="0"
                                @input="updatePeriod(sp.data.periods, i, col)"
                            />
                        </template>
                    </Column>
                    <Column style="min-width: 4rem" :header="t('body.sampleRequest.importPlan.table_header_action')">
                        <template #body="sp">
                            <Button
                                icon="pi pi-trash"
                                text
                                @click="removeItem(sp.data.itemCode)"
                                v-tooltip="t('body.OrderList.delete')"
                            />
                        </template>
                    </Column>
                    <template #empty>
                        <div class="m-6 text-center">{{ t('body.sampleRequest.importPlan.empty_state_message') }}</div>
                    </template>
                    <template #footer>
                        <div class="flex align-items-center gap-2">
                            <div
                                v-if="customer"
                                @click="
                                    Object.keys(errMsg).forEach((key) => {
                                        errMsg[key] = null;
                                    })
                                "
                            >
                                <ProductSelector
                                    icon="pi pi-plus"
                                    :customer="customer"
                                    :label="t('body.sampleRequest.importPlan.add_product_button')"
                                    outlined
                                    v-model:selection="modelSates.saleForecastItems"
                                    @confirm="onChoseProduct"
                                />
                            </div>
                            <Button
                                v-else
                                icon="pi pi-plus"
                                :label="t('body.sampleRequest.importPlan.add_product_button')"
                                outlined
                                @click="showValidateMessage"
                            />
                            <small
                                v-if="submited && !modelSates.saleForecastItems.length"
                                class="text-red-500"
                            >{{ t('body.sampleRequest.importPlan.empty_state_message') }}</small>
                        </div>
                    </template>
                </DataTable>
            </TabPanel>
            <TabPanel :header="t('body.sampleRequest.importPlan.customer_info_tab')">
                <div class="grid">
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">{{ t('body.sampleRequest.importPlan.customer_label') }}</label>
                        <InputText
                            :readonly="customer?.cardName ? true : false"
                            :value="customer?.cardName"
                        ></InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">{{ t('body.sampleRequest.customer.contact_name_label') }}</label>
                        <InputText
                            :readonly="customer?.frgnName ? true : false"
                            :value="customer?.frgnName"
                        ></InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">{{ t('body.sampleRequest.customer.email_label') }}</label>
                        <InputText
                            :readonly="customer?.email ? true : false"
                            :value="customer?.email"
                        ></InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">{{ t('body.sampleRequest.customer.phone_label') }}</label>
                        <InputText
                            :readonly="customer?.phone ? true : false"
                            :value="customer?.phone"
                        ></InputText>
                    </div>
                    <div class="col-12">
                        <SearchAddress
                            :customer="customer"
                            :data="customer ? customer.crD1 : []"
                        ></SearchAddress>
                    </div>
                </div>
            </TabPanel>
        </TabView>
        <template #footer>
            <Button
                @click="onClickCancel"
                :label="t('body.sampleRequest.importPlan.cancel_button')"
                icon="pi pi-times"
                severity="secondary"
            />
            <Button
                :loading="loadingBtn"
                :label="t('body.sampleRequest.importPlan.save_button')"
                icon="pi pi-save"
                @click="onClickSave"
            />
        </template>
    </Dialog>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from "vue";
import { useToast } from "primevue/usetoast";
import { useAuthStore } from "@/Pinia/auth";
import { format } from "date-fns";
import API from "@/api/api-main";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const toast = useToast();
const authStore = useAuthStore();
const visible = defineModel("visible", {
    type: Boolean,
    required: true,
    default: false,
}); 
const props = defineProps({
    header: {
        type: String, 
    },
    getAllData: {
        type: Function,
        required: true,
    },
});

const checkEnable = () => {
    if (modelSates.planName) enableGenrate.value = false;
    else enableGenrate.value = true;
};

const onChangeEndDate = () => {
    generatePlanName();
};
var enableGenrate = ref(true);
const generatePlanName = () => {
    if (!enableGenrate.value) return;
    const startDate = modelSates.startDate
        ? `${format(modelSates.startDate, "MM/yyyy")}`
        : "";
    const endDate = modelSates.endDate ? `${format(modelSates.endDate, "MM/yyyy")}` : "";
    modelSates.planName = `Kế hoạch nhập hàng (${startDate} - ${endDate})`;
};

const currentDate = new Date();
const minFromDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);

const defaultFromDate = minFromDate;

const customer = ref(null);
//lấy dữ liệu khách hàng
const getCustomer = async () => {
    const res = await API.get(`Account/me`);
    if (res.data) customer.value = res.data.user.bpInfo;
};

const DataCustomer = ref(null);
//kiểm tra dữ liệu khách hàng
const CheckValid = () => {
    return submited.value && typeof customer.value != "object";
};
//lấy dữ liệu tìm kiếm khách hàng
const GetCustomer = async (key) => {
    try {
        const res = await API.get(`Customer?search=${key}`);
        if (res.data)
            DataCustomer.value = res.data.items?.filter((cus) => cus.status != "D");
    } catch (error) {}
};

onMounted(() => {
    if (authStore.userType !== "APSP") getCustomer();
});

//khung dữ liệu
const defaultModel = {
    planName: null, // Tên kế hoạch
    notes: null, // Ghi chú
    startDate: defaultFromDate, // Ngày bắt đầu
    endDate: null, // Ngày kết thúc
    status: "P", // Trạng thái
    periodType: "M", // Kế hoạch theo
    customerId: authStore.user?.appUser?.cardId || null,
    saleForecastItems: [],
};

const errMsg = reactive({
    planName: null,
    endDate: null,
});

const modelSates = reactive({ ...defaultModel });

//tính toán số lượng cột rồi gắn giá trị mặc định của cột
const onChangeFomat = () => {
    modelSates.saleForecastItems.forEach((item) => {
        item.periods = Array(columns.value.length)
            .fill()
            .map((_, i) => ({
                periodName: columns.value[i] || "",
                periodIndex: i,
                quantity: 0,
            }));
    });
};

const showValidateMessage = () => {
    errMsg.toDate = null;
    errMsg.endDate = null;
    errMsg.customer = null;
    if (modelSates.toDate == null) {
        errMsg.toDate = t('client.select_start_date');
    }
    if (modelSates.endDate == null) {
        errMsg.endDate = t('client.select_end_date');
    }
    if (!customer.value?.id) {
        errMsg.customer = t('client.select_customer');
    }
};

const resetErrMsg = () => {
    Object.keys(errMsg).forEach((key) => {
        errMsg[key] = null;
    });
};

const onClickCancel = () => {
    visible.value = false;
    Object.keys(modelSates).forEach((key) => {
        modelSates[key] = defaultModel[key];
    });
    modelSates.saleForecastItems = [];
    resetErrMsg();
};

//chọn sản phẩm và gắn giá trị mặc định của cột
const onChoseProduct = (data) => {
    let existingProducts = modelSates.saleForecastItems.map((p) => p.itemCode);
    let newProducts = data.filter((p) => !existingProducts.includes(p.itemCode));
    let updatedProducts = newProducts.map((p) => ({
        ...p,
        periods: Array(columns.value.length)
            .fill()
            .map((_, i) => ({
                periodName: columns.value[i] || "",
                periodIndex: i,
                quantity: 0,
            })),
    }));
    modelSates.saleForecastItems = [...modelSates.saleForecastItems, ...updatedProducts];
};

//tính toán ngày kết thúc
const minToDate = computed(() => {
    let time = new Date(modelSates.startDate);
    if (modelSates.startDate) {
        time.setMonth(modelSates.startDate.getMonth() + 1);
    }
    return time;
});

//tính toán số lượng cột
const columns = computed(() => {
    if (modelSates.startDate && modelSates.endDate && modelSates.periodType) {
        let arrayColumn = getDistanceTime(
            modelSates.startDate,
            modelSates.endDate,
            modelSates.periodType
        );
        return arrayColumn;
    } else {
        return [];
    }
});

//nếu cột thay đổi sẽ gọi hàm onChangeFomat
watch(
    () => columns.value,
    (newVal, oldVal) => {
        if (newVal !== oldVal) {
            onChangeFomat();
        }
    }
);

const onChangeFromDate = (data) => {
    if (modelSates.endDate && isThreeMonthsApart(data, modelSates.endDate)) {
    } else {
        modelSates.endDate = null;
    }
    generatePlanName();
};

const optionsDataColumns = ref([
    { value: "M", label: t('body.promotion.date_option_month') },
    { value: "T", label: t('body.promotion.date_option_week') },
]);

function isThreeMonthsApart(date1, date2) {
    // Lấy năm và tháng của hai ngày
    const year1 = date1.getFullYear();
    const month1 = date1.getMonth();
    const year2 = date2.getFullYear();
    const month2 = date2.getMonth();

    // Tính toán khoảng cách tháng giữa hai ngày
    const monthDifference = (year2 - year1) * 12 + (month2 - month1);

    // Kiểm tra xem khoảng cách có đúng 3 tháng không
    return Math.abs(monthDifference) === 3;
}

//tính toán số lượng cột theo (Tuần, tháng, quý)
function getDistanceTime(date1, date2, by) {
    const result = [];
    const startDate = new Date(date1);
    const endDate = new Date(date2);

    // Đảm bảo rằng date1 nhỏ hơn hoặc bằng date2
    // if (startDate > endDate) [startDate, endDate] = [endDate, startDate];

    switch (by) {
        case "M": // Theo tháng
            while (startDate <= endDate) {
                result.push(`Tháng ${startDate.getMonth() + 1}`);
                startDate.setMonth(startDate.getMonth() + 1);
            }
            break;

        case "Q": // Theo quý
            while (startDate <= endDate) {
                const quarter = Math.floor(startDate.getMonth() / 3) + 1;
                result.push(`Quý ${quarter}`);
                startDate.setMonth(startDate.getMonth() + 3);
            }
            break;

        case "T": // Theo tuần
            const startWeek = getWeekNumber(startDate);
            const endWeek = getWeekNumber(endDate);
            let days = Math.abs(startDate - endDate) / (1000 * 60 * 60 * 24);
            let weekDistance = Math.ceil(days / 7);
            // alert(`${days} - ${weekDistance}`);
            const yearStart = startDate.getFullYear();
            const yearEnd = endDate.getFullYear();

            let week = startWeek;
            for (let i = 1; i <= weekDistance; i++) {
                if (week > 52) {
                    week = 1;
                }
                let str = `Tuần ${week}`;
                week++;
                result.push(str);
            }
            break;

        default:
            throw new Error(
                "Tham số 'by' không hợp lệ. Hãy dùng 'm' cho tháng, 'q' cho quý, hoặc 'w' cho tuần."
            );
    }

    return result;
}

// Hàm tính số thứ tự tuần trong năm
function getWeekNumber(date) {
    const firstDayOfYear = new Date(date.getFullYear(), 0, 1);
    const daysInYear = Math.floor((date - firstDayOfYear) / (24 * 60 * 60 * 1000));
    return Math.ceil((daysInYear + firstDayOfYear.getDay() + 1) / 7);
}

//cập nhật các giá trị của cột
function updatePeriod(periods, index, periodName) {
    if (!periods[index]) {
        periods[index] = { periodName: periodName, periodIndex: index, quantity: 0 };
    } else {
        periods[index].periodName = periodName;
        periods[index].periodIndex = index;
        periods[index].quantity = periods[index].quantity || 0;
    }
}

//kiểm tra dữ liệu
const submited = ref(false);
const hasValidQuantity = computed(() => {
    return modelSates.saleForecastItems.every((item) =>
        item.periods.some((period) => period.quantity > 0)
    );
});
const validate = () => {
    const { planName, startDate, endDate, saleForecastItems } = modelSates;
    const check = Boolean(
        planName &&
            startDate &&
            endDate &&
            saleForecastItems.length &&
            customer.value &&
            hasValidQuantity.value
    );
    if (!check) {
        if (!hasValidQuantity.value) {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Thông tin sản lượng chưa chính xác, vui lòng kiểm tra lại!",
                life: 3000,
            });
        } else {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Vui lòng điền đầy đủ thông tin!",
                life: 3000,
            });
        }
    }

    return check;
};

const loadingBtn = ref(false);
//Gọi api lưu dữ liệu
const onClickSave = async () => {
    submited.value = true;
    if (!validate()) return;
    loadingBtn.value = true;

    const data = {
        planName: modelSates.planName,
        startDate: modelSates.startDate,
        endDate: modelSates.endDate,
        periodType: modelSates.periodType,
        notes: modelSates.notes,
        status: modelSates.status,
        customerId: customer.value.id,
        customerName: customer.value.cardName,
        customerCode: customer.value.cardCode,
        planDate: new Date(),
        saleForecastItems: modelSates.saleForecastItems.map((item) => ({
            itemCode: item.itemCode,
            itemId: item.id,
            itemName: item.itemName,
            periods: [...item.periods],
        })),
    };
    try {
        const res = await API.add(`sale-forecast`, data);
        if (res.data) {
            toast.add({
                severity: "success",
                summary: "Thông báo",
                detail: "Tạo kế hoạch thành công",
                life: 3000,
            });
            onClickCancel();
            props.getAllData();
        }
    } catch (error) {
        console.error(error);
        toast.add({
            severity: "error",
            summary: "Lỗi",
            detail: error.response.data.errors,
            life: 3000,
        });
    } finally {
        loadingBtn.value = false;
    }
};

const onClose = () => {
    onClickCancel();
};

watch(visible, (newVal, oldVal) => {
    submited.value = false;
    if (newVal === false && authStore.userType === "APSP") {
        customer.value = null;
    }
    if (newVal === true) {
        enableGenrate.value = true;
        generatePlanName();
    }
});
//Xoá 1 dòng sản phẩm
const removeItem = (itemCode) => {
    modelSates.saleForecastItems = modelSates.saleForecastItems.filter(
        (item) => item.itemCode !== itemCode
    );
};
</script>
<style scoped>
</style>
