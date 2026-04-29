<template>
    <div class="flex justify-content-between mb-3">
        <div>
            <h4 class="m-0 font-semibold">Chi tiết đề xuất cấp mẫu thử nghiệm</h4>
        </div>
        <div class="flex gap-3">
            <ButtonGoBack/>
            <Button
                v-if="payload.status == 'NHAP'"
                @click="onClickApproval()"
                label="Gửi phê duyệt    "
            />
            <Button
                v-if="payload.status == 'DXN'"
                @click="complete()"
                label="Hoàn thành"
            />
        </div>
    </div>

    <div class="flex justify-content-between card">
        <div class="col-3">
            <div class="flex flex-column gap-2 mb-3">
                <div>Mã yêu cầu</div>
                <InputText
                    v-model="payload.invoiceCode"
                    type="text"
                    disabled
                    placeholder="Mã tự động sinh"
                />
            </div>
            <div class="flex flex-column gap-2">
                <div>Ngày tạo</div>
                <Calendar
                    v-model="payload.docDate"
                    readonly
                    dateFormat="dd/mm/yy"
                    disabled
                ></Calendar>
            </div>
        </div>
        <div class="col-9 flex flex-column justify-content-end">
            <div class="flex flex-wrap gap-2 mb-4">
                <div>Khách hàng</div>
                <div class="flex align-items-center">
                    <RadioButton
                        v-model="customerType"
                        inputId="ingredient1"
                        name="Khách hàng mới"
                        :value="true"
                        readonly
                        @change="
                            () => {
                                payload.internalCust = false;
                                payload.externalCust = true;
                            }
                        "
                    />
                    <label for="ingredient1" class="ml-2"> Khách hàng mới</label>
                </div>
                <div class="flex align-items-center">
                    <RadioButton
                        v-model="customerType"
                        inputId="ingredient2"
                        name="Trong danh sách"
                        :value="false"
                        readonly
                        @change="
                            () => {
                                payload.internalCust = true;
                                payload.externalCust = false;
                            }
                        "
                    />
                    <label for="ingredient2" class="ml-2">Trong danh sách</label>
                </div>
                <div class="ml-7 pl-2">
                    <span class="mr-3">Trạng thái:</span>
                    <Tag
                        :value="getSeverity(payload.status, 'label')"
                        :severity="getSeverity(payload.status, 'severity')"
                    />
                </div>
                <!-- <div>{{ payload.internalCust }} | {{ payload.externalCust }}</div> -->
            </div>
            <div class="flex gap-3">
                <div v-if="customerType" class="flex gap-3">
                    <div class="flex flex-column gap-2">
                        <label>Tên khách hàng</label>
                        <InputText
                            disabled
                            type="text"
                            v-model="payload.cardName"
                            class="w-30rem"
                        />
                    </div>
                    <div class="flex flex-column gap-2">
                        <label>Loại khách hàng</label>
                        <Dropdown
                            v-model="payload.cardType"
                            :options="listCustomer"
                            optionLabel="name"
                            optionValue="code"
                            class="w-20rem"
                            disabled
                        />
                    </div>
                </div>
                <div v-else class="flex flex-column gap-2 w-30rem">
                    <label>Tên khách hàng</label>
                    <!-- {{ (payload.bpId = 1371) }} -->
                    <!-- <CustomerSelector
                        @item-select="onSelectCustomer"
                        v-model="payload.cardId"
                        disabled
                    ></CustomerSelector> -->
                    <InputText
                        disabled
                        type="text"
                        v-model="payload.cardName"
                        class="w-30rem"
                    />
                </div>
                <div class="flex flex-column gap-2">
                    <label>Người tạo</label>
                    <InputText v-model="payload.userName" disabled></InputText>
                </div>
            </div>
        </div>
    </div>

    <div class="card">
        <DataTable :value="product" showGridlines scrollable scrollHeight="535px">
            <Column header="#" class="w-1rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column header="Mã sản phẩm" field="itemCode" style="width: 10rem"></Column>
            <Column header="Tên sản phẩm" field="itemName"></Column>
            <Column header="Số lượng (lít)" field="quantity" class="w-10rem text-right">
                <template #body="{ data }">
                    {{ Intl.NumberFormat().format(data.quantity) }}
                </template>
            </Column>
            <Column
                v-if="payload.status !== 'DXL'"
                header="Kết quả thử nghiệm"
                style="width: 13rem"
            >
                <template #body="{ data }">
                    <Dropdown
                        v-if="payload.status == 'DXN'"
                        v-model="data.result"
                        optionLabel="label"
                        optionValue="code"
                        :options="kqOption"
                        placeholder="Chọn kết quả"
                        class="w-12rem"
                    ></Dropdown>
                    <div v-else>
                        {{ kqOption.find((item) => item.code == data.result)?.label }}
                    </div>
                </template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column colspan="3" footer="Tổng sản lượng:"> </Column>
                    <Column :footer="totalLitter" class="text-right"></Column>
                    <Column v-if="payload.status !== 'DXL'"></Column>
                </Row>
            </ColumnGroup>
        </DataTable>
    </div>
    <Loading v-if="loading"></Loading>
</template>

<script setup>
import { ref, onBeforeMount, computed } from "vue";
import API from "@/api/api-main";
import Dropdown from "primevue/dropdown";
import { useGlobal } from "../../../services/useGlobal";
import { useRoute } from "vue-router";

const { toast, FunctionGlobal } = useGlobal();
const customerType = ref(true);
const DataCustomer = ref([]);
const totalCustomer = ref(0);
const visibleCustomer = ref(false);
const CustomerTem = ref();
const Customer = ref("");
const loading = ref(false);
const product = ref([]);

const stts = {
    DXL: {
        label: "Đang xử lý",
        severity: "warning",
    },
    DXN: {
        label: "Đã xác nhận",
        severity: "info",
    },
    HT: {
        label: "Hoàn thành",
        severity: "success",
    },
    HUY: {
        label: "Từ chối",
        severity: "danger",
    },
    NHAP: {
        label: "Nháp",
        severity: "secondary",
    },
};
const getSeverity = (status, prop) => {
    return stts[status]?.[prop] || "";
};

const totalLitter = computed(() => {
    let sum = product.value.reduce((sum, item) => {
        return item.quantity ? sum + item.quantity : sum;
    }, 0);
    return Intl.NumberFormat().format(sum);
});

const kqOption = [
    {
        code: "OK",
        label: "Đạt",
    },
    {
        code: "NOTOK",
        label: "Không đạt",
    },
];

const payload = ref({
    id: 0,
    invoiceCode: "",
    internalCust: true,
    externalCust: true,
    cardId: 0,
    cardCode: "",
    cardName: "",
    cardType: "",
    docDate: new Date(),
    rfS1: [
        {
            id: 0,
            fatherId: 0,
            itemId: 0,
            itemCode: "",
            itemName: "",
            quantity: 0,
            result: "",
        },
    ],
    userId: 0,
    userName: "",
});

const listCustomer = [
    {
        name: "Khách hàng công nghiệp",
        code: "IN",
    },
    {
        name: "Khách hàng xuất khẩu",
        code: "EX",
    },
    {
        name: "Nhà phân phối",
        code: "DI",
    },
];

const onClickApproval = () => {
    payload.value.status = "DXL";
    API.update(`RequestOfExample/${payload.value.id}`, payload.value).then((res) => {});
};

const complete = async () => {
    const nullLine = payload.value.rfS1
        .map((item) => item.result)
        .filter((result) => !result).length;
    if (nullLine > 0) {
        toast.add({
            summary: "Cảnh báo",
            detail: "Kết quả thử nghiệp không được để trống",
            severity: "warn",
            life: 3000,
        });
        return;
    }
    API.update(`RequestOfExample/${route.params.id}`, payload.value)
        .then((res) => {
            toast.add({
                summary: "Thành công",
                detail: "Cập nhật dữ liệu thành công",
                severity: "success",
                life: 3000,
            });
            window.history.back();
        })
        .catch(() => {
            toast.add({
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                severity: "error",
                life: 3000,
            });
        });
};

const testResult = [
    { name: "Đạt", code: "OB" },
    { name: "Không đạt", code: "NA" },
];

const onSelectedProduct = (e) => {
    e.forEach((item) => {
        // Kiểm tra nếu itemId đã có trong product.value
        const existingProduct = product.value.find((p) => p.itemId === item.id);

        if (existingProduct) {
            existingProduct.quantity += 1;
        } else {
            product.value.push({
                itemId: item.id,
                itemCode: item.itemCode,
                itemName: item.itemName,
                quantity: 1,
            });
        }
    });
};

const goBack = () => {
    window.history.back();
};

const onSelectCustomer = (val) => {

    payload.value.cardCode = val.cardCode;
    payload.value.cardName = val.cardName;
};

const route = useRoute();
onBeforeMount(() => {
    API.get(`RequestOfExample/${route.params.id}`).then((res) => {
        payload.value = res.data.items;
        payload.value.docDate = payload.value.docDate
            ? new Date(payload.value.docDate)
            : null;
        product.value = payload.value.rfS1;

        // Nếu là KH mới
        if (payload.value.internalCust) {
            customerType.value = false;
        } else {
            customerType.value = true;
        }
    });
});
</script>
