<template>
    <Dialog @hide="onClose" v-model:visible="visible" header="Cập nhật kế hoạch nhập hàng" modal class="w-10">
        <div class="grid m-0 mt-0">
            <div class="grid col-9">
                <div class="flex flex-column field col-3 py-0">
                    <label class="font-semibold" for="">Mã kế hoạch <span class="text-red-500">*</span></label>
                    <InputText v-model="modelSates.planCode" placeholder="Hệ thống tự động sinh" disabled></InputText>
                </div>
                <div class="flex flex-column field col-9 py-0">
                    <label class="font-semibold" for="">Tên kế hoạch <span class="text-red-500">*</span></label>
                    <InputText v-model="modelSates.planName" placeholder="Tên kế hoạch"
                        :invalid="submited && !modelSates.planName"></InputText>
                    <small v-if="submited && !modelSates.planName" class="text-red-500">Vui lòng điền tên kế
                        hoạch</small>
                </div>

                <div class="flex flex-column field col-2 py-0" :class="{
                    'col-4': authStore.userType !== 'APSP',
                }">
                    <label class="font-semibold" for="">Ngày bắt đầu</label>
                    <Calendar @date-select="onChangeFromDate" v-model="modelSates.startDate" :minDate="minFromDate"
                        dateFormat="dd/mm/yy"></Calendar>
                </div>
                <div class="flex flex-column field col-2 py-0" :class="{
                    'col-4': authStore.userType !== 'APSP',
                }">
                    <label class="font-semibold" for="">Ngày kết thúc <span class="text-red-500">*</span></label>
                    <Calendar :minDate="minToDate" v-model="modelSates.endDate" dateFormat="dd/mm/yy"
                        :invalid="submited && !modelSates.endDate"></Calendar>
                    <small v-if="submited && !modelSates.endDate" class="text-red-500">Vui lòng chọn ngày kết
                        thúc</small>
                </div>
                <div class="flex flex-column field col-2 py-0" :class="{
                    'col-4': authStore.userType !== 'APSP',
                }">
                    <label class="font-semibold" for="">Kế hoạch theo</label>
                    <Dropdown @change="onChangeFomat" v-model="modelSates.periodType" :options="optionsDataColumns"
                        optionLabel="label" optionValue="value"></Dropdown>
                </div>
                <div class="flex flex-column field col-6 py-0" v-if="authStore.userType === 'APSP'">
                    <label class="font-semibold" for="">Khách hàng <span class="text-red-500">*</span></label>
                    <InputGroup>
                        <AutoComplete :pt:input:class="'w-30rem'" :invalid="CheckValid()" v-model="customer"
                            :suggestions="DataCustomer" optionLabel="cardName" @complete="GetCustomer(customer)">
                            <template #option="data">
                                <div class="flex gap-2 align-items-center">
                                    <Avatar icon="pi pi-user" class="mr-2" size="large"
                                        style="background-color: #ece9fc; color: #2a1261" shape="circle" />
                                    <div class="flex flex-column gap-1">
                                        <span class="font-bold">{{
                                            data.option.cardName
                                        }}</span>
                                        <span class="text-gray-500">{{
                                            data.option.email
                                        }}</span>
                                    </div>
                                </div>
                            </template>
                        </AutoComplete>
                        <InputGroupAddon>
                            <i class="pi pi-search"></i>
                        </InputGroupAddon>
                    </InputGroup>
                    <small v-if="submited && !customer" class="text-red-500">Vui lòng chọn khách hàng</small>
                </div>
            </div>

            <div class="grid col">
                <div class="flex flex-column field col py-0">
                    <label class="font-semibold" for="">Ghi chú</label>
                    <Textarea v-model="modelSates.notes" rows="3" class="w-full h-full"
                        placeholder="Nội dung ghi chú"></Textarea>
                </div>
            </div>
        </div>
        <TabView class="card p-0 overflow-hidden" value="0">
            <TabPanel header="Sản phẩm">
                <small v-if="submited && !hasValidQuantity" class="text-red-500">Mỗi sản phẩm phải ít nhất 1 cột có số
                    lượng lớn hơn 0</small>
                <DataTable :value="modelSates.saleForecastItems.filter((item) => item.status !== 'D')
                    " scrollable showGridlines>
                    <Column header="#" style="min-width: 5rem">
                        <template #body="sp">
                            {{ sp.index + 1 }}
                        </template>
                    </Column>
                    <Column field="itemName" header="Tên hàng hóa" style="min-width: 30rem"></Column>
                    <Column header="Đơn vị tính" style="min-width: 10rem">
                        <template #body="sp">
                            {{
                                sp.data.packing?.name
                                    ? sp.data.packing.name
                                    : sp.data.ugpName
                            }}
                        </template>
                    </Column>
                    <Column v-for="(col, i) in columns" :key="i" :header="col" style="min-width: 7rem">
                        <template #body="sp">
                            <InputNumber v-model="sp.data.periods[i].quantity"
                                :pt:input:root:class="'w-7rem text-right'" placeholder="0" :min="0"
                                @input="updatePeriod(sp.data.periods, i, col, sp.data.id)" />
                        </template>
                    </Column>
                    <Column style="min-width: 4rem" header="Hành động">
                        <template #body="sp">
                            <Button icon="pi pi-trash" text @click="removeItem(sp.data.itemCode)" v-tooltip="'Xoá'" severity="danger" />
                        </template>
                    </Column>
                    <template #empty>
                        <div class="m-6 text-center">Chưa có sản phẩm được chọn</div>
                    </template>
                    <template #footer>
                        <div class="flex align-items-center gap-2">
                            <ProductSelector v-if="modelSates.endDate" icon="pi pi-plus" label="Chọn sản phẩm" outlined
                                v-model:selection="modelSates.saleForecastItems" @confirm="onChoseProduct" />
                            <Button v-else icon="pi pi-plus" label="Chọn sản phẩm" outlined
                                @click="showMessage"/>
                            <small v-if="submited && !modelSates.saleForecastItems.length" class="text-red-500">Vui lòng
                                chọn sản phẩm</small>
                        </div>
                    </template>
                </DataTable>
            </TabPanel>
            <TabPanel header="Thông tin khách hàng">
                <!-- <div class="font-bold mb-3 text-lg">Thông tin khách hàng</div> -->
                <div class="grid">
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">Công ty</label>
                        <InputText :readonly="customer?.cardName ? true : false" :value="customer?.cardName">
                        </InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">Tên khác</label>
                        <InputText :readonly="customer?.frgnName ? true : false" :value="customer?.frgnName">
                        </InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">Email</label>
                        <InputText :readonly="customer?.email ? true : false" :value="customer?.email"></InputText>
                    </div>
                    <div class="col-6 flex flex-column gap-2 pb-0">
                        <label for="">Số điện thoại</label>
                        <InputText :readonly="customer?.phone ? true : false" :value="customer?.phone"></InputText>
                    </div>
                    <div class="col-12">
                        <SearchAddress :customer="customer" :data="customer ? customer.crD1 : []"></SearchAddress>
                    </div>
                </div>
            </TabPanel>
        </TabView>
        <template #footer>
            <Button @click="onClickCancel" label="Hủy" icon="pi pi-times" severity="secondary" />
            <Button :loading="loadingBtn" label="Lưu" icon="pi pi-save" @click="onClickSave" />
        </template>
    </Dialog>
</template>
<script setup>
import { ref, reactive, computed, onMounted, watch, watchEffect } from "vue";
import { useToast } from "primevue/usetoast";
import { useAuthStore } from "@/Pinia/auth";
import API from "@/api/api-main";
import { merge } from "lodash";
const toast = useToast();
const authStore = useAuthStore();
const visible = defineModel("visible", {
    type: Boolean,
    required: true,
    default: false,
});
const props = defineProps({
    getAllData: {
        type: Function,
        required: true,
    },
    data: {
        type: Object,
        required: true,
    },
});

const currentDate = new Date();
const minFromDate = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);
const defaultFromDate = minFromDate;

const customer = ref(null);
const getCustomer = async (id) => {
    const res = await API.get(`Customer/${id}`);
    if (res.data) customer.value = res.data;
};
const DataCustomer = ref(null);
const CheckValid = () => {
    return submited.value && typeof customer.value != "object";
};
const GetCustomer = async (key) => {
    try {
        const res = await API.get(`Customer?search=${key}`);
        if (res.data) DataCustomer.value = res.data.items;
    } catch (error) { }
};

const modelSates = ref({
    planCode: null, // Mã kế hoạch
    planName: null, // Tên kế hoạch
    notes: null, // Ghi chú
    startDate: defaultFromDate, // Ngày bắt đầu
    endDate: null, // Ngày kết thúc
    status: "P", // Trạng thái
    periodType: "M", // Kế hoạch theo
    customerId: null, // Id khách hàng
    saleForecastItems: [],
});

watch(
    () => props.data,
    async (newVal) => {
        const processedData = {
            ...newVal,
            startDate: newVal.startDate ? new Date(newVal.startDate) : null,
            endDate: newVal.endDate ? new Date(newVal.endDate) : null,
        };
        merge(modelSates.value, processedData);

        modelSates.value.saleForecastItems.forEach((item) => {
            item.status = null;
        });
        await getCustomer(newVal.customerId);
    }
);
watch(
    () => visible,
    (newVal, oldVal) => {
        submited.value = false;
    }
);
const onChangeFomat = () => {
    modelSates.value.saleForecastItems.forEach((item) => {
        if (!item.originalQuantities) {
            if (modelSates.value.periodType === "M") {
                item.originalQuantities = {
                    M: item.periods.map((p) => p.quantity),
                    T: item.periods.map((p) => 0),
                };
            } else {
                item.originalQuantities = {
                    M: item.periods.map((p) => 0),
                    T: item.periods.map((p) => p.quantity),
                };
            }
        }
        const currentPeriodType = modelSates.value.periodType;
        const existingPeriodsMap = new Map(item.periods.map((p) => [p.periodName, p]));

        item.periods = Array(columns.value.length)
            .fill()
            .map((_, i) => {
                const periodName = columns.value[i] || "";
                const existingPeriod = existingPeriodsMap.get(periodName);
                return {
                    periodName: periodName,
                    periodIndex: i,
                    quantity: item.originalQuantities[currentPeriodType][i] || 0,
                    id: item.periods[i]?.id || null,
                };
            });
    });
};

const onClickCancel = () => {
    visible.value = false;
    Object.keys(modelSates.value).forEach((key) => {
        modelSates.value[key] = modelSates[key];
    });
    modelSates.value.saleForecastItems = [];
};

const onChoseProduct = (data) => {
    let existingProducts = modelSates.value.saleForecastItems.map((p) => p.itemCode);
    let newProducts = data.filter((p) => !existingProducts.includes(p.itemCode));
    let updatedProducts = newProducts.map((p) => ({
        ...p,
        status: "A",
        itemId: p.id,
        id: null,
        periods: Array(columns.value.length)
            .fill()
            .map((_, i) => ({
                periodName: columns.value[i] || "",
                periodIndex: i,
                quantity: 0,
            })),
    }));
    modelSates.value.saleForecastItems = [
        ...modelSates.value.saleForecastItems,
        ...updatedProducts,
    ];
};

const minToDate = computed(() => {
    let time = new Date(modelSates.value.startDate);
    time.setMonth(modelSates.value.startDate.getMonth() + 2);
    return time;
});

const columns = computed(() => {
    if (
        modelSates.value.startDate &&
        modelSates.value.endDate &&
        modelSates.value.periodType
    ) {
        let arrayColumn = getDistanceTime(
            modelSates.value.startDate,
            modelSates.value.endDate,
            modelSates.value.periodType
        );
        return arrayColumn;
    } else {
        return [];
    }
});

watch(
    () => columns.value,
    (newVal, oldVal) => {
        if (newVal !== oldVal) {
            onChangeFomat();
        }
    }
);

const onChangeFromDate = (data) => {
    if (modelSates.value.endDate && isThreeMonthsApart(data, modelSates.value.endDate)) {
    } else {
        modelSates.value.endDate = null;
    }
};

const optionsDataColumns = ref([
    { value: "M", label: "Tháng" },
    { value: "T", label: "Tuần" },
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

function updatePeriod(periods, index, periodName, id) {
    if (!periods[index]) {
        periods[index] = {
            periodName: periodName,
            periodIndex: index,
            quantity: 0,
            id: id,
        };
    } else {
        periods[index].periodName = periodName;
        periods[index].periodIndex = index;
        periods[index].quantity = periods[index].quantity || 0;
        modelSates.value.saleForecastItems.forEach((item) => {
            if (item.id === id && id !== null) {
                item.status = "U";
            }
        });
    }
}

const submited = ref(false);
const hasValidQuantity = computed(() => {
    return modelSates.value.saleForecastItems.every((item) =>
        item.periods.some((period) => period.quantity > 0)
    );
});
const validate = () => {
    const { planName, endDate, saleForecastItems } = modelSates.value;
    const check = Boolean(
        planName &&
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

const onClickSave = async () => {
    submited.value = true;
    if (!validate()) return;
    if (
        modelSates.value.saleForecastItems?.filter((item) => item.status !== "D").length <
        1
    ) {
        alert("Danh sách sản phẩm không được để trống");
        return;
    }
    loadingBtn.value = true;
    const formData = {
        id: modelSates.value.id,
        planName: modelSates.value.planName,
        planCode: modelSates.value.planCode,
        notes: modelSates.value.notes,
        startDate: modelSates.value.startDate,
        endDate: modelSates.value.endDate,
        periodType: modelSates.value.periodType,
        customerId: customer.value.id,
        userId: modelSates.value.userId,
        saleForecastItems: modelSates.value.saleForecastItems.map((item) => ({
            ...(item.id && { id: item.id }),
            itemCode: item.itemCode,
            itemId: item.itemId,
            itemName: item.itemName,
            status: item.status,
            periods: [...item.periods],
        })),
    };
    try {
        const res = await API.update(`sale-forecast/${props.data.id}`, formData);
        if (res.data) {
            toast.add({
                severity: "success",
                summary: "Thông báo",
                detail: "Cập nhật kế hoạch thành công",
                life: 3000,
            });
            onClickCancel();
            props.getAllData();
        }
    } catch (error) {
        console.error(error);
    } finally {
        loadingBtn.value = false;
    }
};

const removeItem = (itemCode) => {
    modelSates.value.saleForecastItems.forEach((item) => {
        if (item.itemCode === itemCode) {
            item.status = "D";
        }
    });
};
</script>
