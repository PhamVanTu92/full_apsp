<script setup>
import { ref, onBeforeMount, computed, watch } from "vue";
import { useRoute } from "vue-router";
import API from "@/api/api-main";
import format from "@/helpers/format.helper";
// import StepHistory from "@/components/StepHistory.vue";
import router from "@/router";
import { useGlobal } from "@/services/useGlobal";
import { uniqBy } from "lodash";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const { toast, FunctionGlobal } = useGlobal();
const Route = useRoute();
const props = defineProps(["id", "transType"]);
const DataProduct = ref([]);
const checkDebt = ref(false);
const User = ref({});
const enableEditNote = ref(false);
const sellerNote = ref("");
const loadingNote = ref(false);
const filesInput = ref(null);
const filesAllow = ["jpg", "jpeg", "png", "pdf", "doc", "docx", "xls", "xlsx", "txt"];
const onClickFilesChungTuGiaoHang = () => {
    filesInput.value.click();
};
const chungChiGiaoHangs = ref([]);

const onClickGiaoHang = async () => {
    if (chungChiGiaoHangs.value.length < 1) {
        FunctionGlobal.$notify("W", "Vui lòng thêm tài liệu chứng từ giao hàng!", toast);
        return;
    }
    try {
        const body = new FormData();
        chungChiGiaoHangs.value.forEach((el) => {
            body.append("AttachFile", el.file);
        });
        // await API.add(`PurchaseOrder/${Route.params.id}/AttDocuments`, body);
        // await API.get(`PurchaseOrder/${Route.params.id}/change-status/DGH`);
        await API.update(`PurchaseOrder/${Route.params.id}/change-status/DGH`, body);
        await GetOrderDetail();
    } catch (error) {
        FunctionGlobal.$notify("E", "Đã có lỗi xảy ra", toast);
    }
};

const onClickHoanThanh = async () => {
    try {
        await API.update(`PurchaseOrder/${Route.params.id}/change-status/DHT`);
        await GetOrderDetail();
        FunctionGlobal.$notify("S", "Đã hoàn thành đơn hàng!", toast);
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    }
};

const onChangeFilesChungTuGiaoHang = async (e) => {
    if (e.target.files.length < 1) return;
    const file = e.target.files[0];
    const fileExtention = file.name.split(".").pop().toLowerCase();
    if (!filesAllow.includes(fileExtention)) {
        alert("File đã chọn không đúng định dạng, vui lòng chọn file khác!");
        return;
    }
    chungChiGiaoHangs.value.push({
        name: file.name,
        file: file,
    });
    filesInput.value.value = null;
};

const checkPermission = () => {
    const user = JSON.parse(localStorage.getItem("user"));
    if (!user) return false;
    let roles = user.appUser.userRoles?.map((r) => r.role.name);
    if (roles.includes("admin") || roles.includes("customer_service")) return true;
    else return false;
};

const onClickEditNote = () => {
    enableEditNote.value = !enableEditNote.value;
    sellerNote.value = DataProduct.value.sellerNote;
    setTimeout(() => {
        document.getElementById("sellerNote").focus();
    }, 0);
};
const onClickSaveNote = async () => {
    loadingNote.value = true;
    try {
        let body = { sellerNote: sellerNote.value?.trim() };
        const res = await API.patch(`PurchaseOrder/${Route.params.id}`, body);
        if (res.status === 200) {
            FunctionGlobal.$notify("S", "Cập nhật ghi chú thành công!", toast);
            DataProduct.value.sellerNote = sellerNote.value?.trim();
            enableEditNote.value = false;
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loadingNote.value = false;
    }
};
const onClickCancelNote = () => {
    enableEditNote.value = false;
};

onBeforeMount(() => {
    GetOrderDetail();
});
const GetOrderDetail = async () => {
    const orderId = props.id || Route.params.id;
    let endpoint =
        orderId && props.transType === 1250000001
            ? `PurchaseRequest/${orderId}`
            : `PurchaseOrder/${orderId}`;

    try {
        const res = await API.get(endpoint);
        if (res.data) {
            DataProduct.value = res.data.item;
            GetUser(DataProduct.value.cardId);
        }
    } catch (error) {}
};
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};

const GetUser = async (id) => {
    try {
        const res = await API.get(`Customer/${id}`);
        User.value = res.data;
    } catch (error) {
        console.error(error);
    }
};
const userLocation = (user, type) => {
    if (!user.crD1 || user.crD1.length === 0) {
        return "";
    }
    const locationData = user.crD1.find((el) => el.type === type);
    return locationData ? `${locationData.locationName} - ${locationData.areaName}` : "";
};
const totalVat = computed(() => {
    if (DataProduct.value.itemDetail) {
        return DataProduct.value.itemDetail.reduce((total, val) => {
            return total + val.vatAmount;
        }, 0);
    }
});
const stt = {
    DXL: {
        label: t('body.status.DXL'),
        class: "text-yellow-700 bg-yellow-200",
    },
    DXN: {
        class: "text-teal-700 bg-teal-200",
        label: t('body.status.DXN'),
    },
    HUY: {
        class: "text-red-700 bg-red-200",
        label: t('body.status.DH'),
    },
    HUY2: {
        class: "text-red-700 bg-red-200",
        label: t('body.status.DH'),
    },
    DGH: {
        class: "text-blue-700 bg-blue-200",
        label: t('body.status.DGH'),
    },
    DHT: {
        class: "text-green-700 bg-green-200",
        label: t('body.status.DHT'),
    },
    CTT: {
        class: "text-yellow-50 bg-yellow-700",
        label: t('body.status.CTT'),
    },
    TTN: {
        class: "text-yellow-700 bg-white border-1 border-yellow-700",
        label: t('body.status.TTN'),
    },
    CXN: {
        class: "text-orange-700 bg-white border-1 border-orange-700",
        label: t('body.status.CXN'),
    },
};
const formatStatus = (data) => {
    return stt[data] || { class: "surface-100", label: data };
};

const checkDisabled = () => {
    if (DataProduct.value.approval?.status === "A" && checkDebt.value == false)
        return false;
    if (
        checkDebt.value &&
        DataProduct.value.status !== "DXN" &&
        DataProduct.value.approval?.status !== "A"
    ) {
        return false;
    }
    return true;
};

const ApproveOrder = async (id) => {
    if (!DataProduct.value.approval || DataProduct.value.approval?.status == "R") {
        try {
            const res = await API.add(`Approval/action-purchase/${Route.params.id}`);

            router.push(`/approval-setup/order-approval/${res.data.id}`);
        } catch (err) {}
        return;
    } else {
        router.push(`/approval-setup/order-approval/${id}`);
    }
    // if (!id) {
    //     try {
    //         const res = await API.add(`Approval/action-purchase/${Route.params.id}`);
    //         if (res.status === 200) {
    //             FunctionGlobal.$notify("S", "Phê duyệt thành công!", toast);
    //             router.push(`/approval-setup/order-approval/${res.data.id}`);
    //         }
    //     } catch (error) {
    //         FunctionGlobal.$notify("E", error, toast);
    //     } finally {
    //         GetOrderDetail();
    //     }
    // } else {
    //     router.push(`/approval-setup/order-approval/${id}`);
    // }
};
import { useConfirm } from "primevue/useconfirm";
const confirm = useConfirm();
const onClickConfirmOrder = (stt) => {
    confirm.require({
        message: "Bạn có muốn xác nhận đơn hàng này?",
        header: "Xác nhận",
        rejectClass: "p-button-secondary",
        rejectLabel: "Đóng",
        acceptLabel: "Xác nhận",
        accept: () => {
            ConfirmOrder("DXN");
        },
    });
};
const ConfirmOrder = async (stt) => {
    try {
        const res = await API.update(
            `PurchaseOrder/${Route.params.id}/change-status/${stt}`
        );
        if (res.status == 200) {
            FunctionGlobal.$notify("S", "Xác nhận thành công!", toast);
            GetOrderDetail();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    }
};
watch(
    Route,
    () => {
        GetOrderDetail();
    },
    { immediate: true }
);

const gifts = computed(() => {
    return (
        DataProduct.value.itemDetail?.filter(
            (item) => item.type == "KH" || item.type == "VPKM"
        ) || []
    );
});
const groupItem = (data) => {
    const grouped = Object.values(
        data.reduce((acc, item) => {
            if (!acc[item.itemCode]) {
                acc[item.itemCode] = {
                    itemCode: item.itemCode,
                    quantity: 0,
                    uomName: item.uomName,
                    itemName: item.itemName,
                };
            }
            acc[item.itemCode].quantity += item.quantity;
            return acc;
        }, {})
    );
    return grouped;
};

const onClickDownloadFile = (file) => {
    var a = document.createElement("a");
    a.href = file.filePath;
    a.target = "_blank";
    a.download = file.fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};

const incotermTypes = [
    {
        code: "EXW",
        name: "EXW (Ex Works)",
    },
    {
        code: "FCA",
        name: "FCA (Free Carrier)",
    },
    {
        code: "CPT",
        name: "CPT (Carriage Paid To)",
    },
    {
        code: "CIP",
        name: "CIP (Carriage and Insurance Paid To)",
    },
    {
        code: "DAP",
        name: "DAP (Delivered At Place)",
    },
    {
        code: "DPU",
        name: "DPU (Delivered at Place Unloaded)",
    },
    {
        code: "DDP",
        name: "DDP (Delivered Duty Paid)",
    },
    {
        code: "FAS",
        name: "FAS (Free Alongside Ship)",
    },
    {
        code: "FOB",
        name: "FOB (Free On Board)",
    },
    {
        code: "CFR",
        name: "CFR (Cost and Freight",
    },
    {
        code: "CIF",
        name: "CIF (Cost, Insurance, and Freight)",
    },
];
const incotermType = (code) => {
    return incotermTypes.find((el) => el.code == code)?.name || "Unknown";
};

const ptttLabel = ref({
    PayNow: "Thanh toán ngay",
    PayCredit: "Công nợ tín chấp",
    PayGuarantee: "Công nợ bảo lãnh",
});

const isExceedDebt = ref(false);
const getInvoices = () => {
    let result = [];
    let code = DataProduct.value.invoiceCode;
    if (DataProduct.value?.paymentInfo?.totalDebt) {
        result.push({
            invoiceNumber: code,
            paymentMethodName: "Công nợ - Tín chấp",
            amountOverdue: DataProduct.value?.paymentInfo?.totalDebt,
            isInvoice: false,
            status: DataProduct.value?.status,
        });
    }
    if (DataProduct.value?.paymentInfo?.totalDebtGuarantee) {
        result.push({
            invoiceNumber: code,
            paymentMethodName: "Công nợ - Bảo lãnh",
            amountOverdue: DataProduct.value?.paymentInfo?.totalDebtGuarantee,
            isInvoice: false,
            status: DataProduct.value?.status,
        });
    }
    return result;
};

const currencySymbol = computed(() => {
    let result = {
        currency: "VND",
        symbol: "đ",
    };
    if (DataProduct.value.currency == "USD") {
        result.currency = "USD";
        result.symbol = "$";
    }

    return result;
});

const files = ref([]);
const onSeletDocs = (e, index) => {

    // files.value.push(e.files[0]);
    DataProduct.value.attachFile[index].localFile = e.files[0];
};

const onCancel = () => {
    DataProduct.value.attachFile?.forEach((row) => {
        delete row.localFile;
    });
};

const loadingDocs = ref(false);
const onUploadDocs = () => {

    loadingDocs.value = true;
    const formData = new FormData();
    const _document = {
        id: DataProduct.value.id,
        attachFile: [
            // {
            //     "id": 2191,
            //     "filename": "0f5be017-3fdb-4be5-bf50-cc7a8b0f982d.xls",
            //     "note": "abc",
            //     "memo": "",
            //     "fatherId": 4974,
            //     "status": "U"
            // },
        ],
    };
    const data = DataProduct.value.attachFile.filter((row) => row.localFile);
    data.forEach((row) => {
        formData.append("attachFile", row.localFile);
        _document.attachFile.push({
            id: row.id,
            filename: row.localFile.name,
            note: row.note,
            memo: row.memo,
            fatherId: row.fatherId,
            status: "U",
        });
    });
    formData.append("document", JSON.stringify(_document));

    API.update(`PurchaseOrder/${_document.id}`, formData)
        .then((res) => {
            FunctionGlobal.$notify("S", "Cập nhật tài liệu thành công", toast);
        })
        .catch((error) => {
            FunctionGlobal.$notify("E", "Cập nhật tài liệu thất bại", toast);
        })
        .finally(() => {
            loadingDocs.value = false;
            onCancel();
            GetOrderDetail();
        });
};

const loadingHDH = ref(false);
const onClickHDH = () => {
    loadingHDH.value = true;
    API.update(`PurchaseOrder/${DataProduct.value.id}/change-status/HUY2`)
        .then((res) => {
            GetOrderDetail();
        })
        .catch((error) => {})
        .finally(() => {
            loadingHDH.value = false;
        });
};
const loadingXNDH = ref(false);
const onClickXNDH = () => {
    loadingXNDH.value = true;
    API.add(`PurchaseOrder/${DataProduct.value.id}/send-payment`)
        .then((res) => {
            GetOrderDetail();
        })
        .catch((error) => {})
        .finally(() => {
            loadingXNDH.value = false;
        });
};
</script>
<template>
    <div v-if="DataProduct">
        <div class="grid align-items-center" v-if="!props.id">
            <div class="col-12">
                <div class="flex gap-2 justify-content-between">
                    <h4 class="m-0 font-bold">Chi tiết đơn hàng</h4>
                    <ButtonGoBack/>
                </div>
            </div>
        </div>
        <div class="grid">
            <div class="col-12">
                <div class="card p-0">
                    <div class="p-4 pb-0">
                        <div class="grid justify-content-between">
                            <div class="col-7">
                                <div class="flex flex-column gap-2">
                                    <div class="flex gap-3">
                                        <span>Mã khách hàng:</span
                                        ><strong>{{
                                            DataProduct.cardCode
                                                ? DataProduct.cardCode
                                                : "--"
                                        }}</strong>
                                    </div>
                                    <div class="flex align-items-center gap-3">
                                        <span>Tên khách hàng:</span>
                                        <router-link
                                            class="text-primary font-semibold hover:underline"
                                            :to="{
                                                name: 'agencyCategory-detail',
                                                params: { id: DataProduct.cardId },
                                            }"
                                            >{{
                                                DataProduct?.cardName || "--"
                                            }}</router-link
                                        >

                                        <!-- {{ isExceedDebt }} -->
                                    </div>
                                    <div class="flex gap-3 align-items-center">
                                        <div>
                                            <span class="mr-3">Trạng thái đơn hàng:</span>
                                            <!-- {{ DataProduct.status }} -->
                                            <Tag
                                                :class="
                                                    formatStatus(DataProduct.status)
                                                        ?.class
                                                "
                                                :value="
                                                    formatStatus(DataProduct.status)
                                                        ?.label
                                                "
                                            />
                                        </div>
                                    </div>
                                    <div
                                        v-if="DataProduct.isIncoterm"
                                        class="flex gap-3 align-items-center"
                                    >
                                        <span
                                            >Incoterm 2020 -
                                            {{
                                                incotermType(DataProduct.incotermType)
                                            }}</span
                                        >
                                    </div>
                                </div>
                            </div>
                            <div class="col-3">
                                <div class="flex flex-column gap-3">
                                    <div class="flex gap-3 justify-content-between">
                                        <span
                                            >Mã đơn hàng:
                                            <strong>{{
                                                DataProduct.invoiceCode
                                            }}</strong></span
                                        >
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Thời gian tạo:</span
                                        ><strong>{{
                                            format.DateTime(DataProduct.docDate).time +
                                            " " +
                                            format.DateTime(DataProduct.docDate).date
                                        }}</strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Người tạo:</span
                                        ><strong>{{
                                            DataProduct?.author?.fullName
                                        }}</strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>
                                            {{
                                                DataProduct.objType === 22
                                                    ? `Thời gian nhận:`
                                                    : `Thời gian lấy hàng`
                                            }} </span
                                        ><strong>{{
                                            DataProduct.deliveryTime
                                                ? format.DateTime(
                                                      DataProduct.deliveryTime
                                                  ).time +
                                                  " " +
                                                  format.DateTime(
                                                      DataProduct.deliveryTime
                                                  ).date
                                                : "--"
                                        }}</strong>
                                    </div>
                                </div>
                            </div>
                            <div class="col-2 flex justify-content-end">
                                <div>
                                    <KiemTraCongNo
                                        v-model:isExceedDebt="isExceedDebt"
                                        :bpId="DataProduct.cardId"
                                        :payCredit="
                                            DataProduct?.paymentInfo?.totalDebt || 0
                                        "
                                        :payGuarantee="
                                            DataProduct?.paymentInfo
                                                ?.totalDebtGuarantee || 0
                                        "
                                        :invoices="getInvoices()"
                                    />
                                </div>
                            </div>
                        </div>
                        <div class="grid">
                            <div class="col-12">
                                <div class="grid justify-content-between">
                                    <div class="col-12">
                                        <DataTable
                                            showGridlines
                                            :value="
                                                DataProduct?.itemDetail?.filter(
                                                    (item) =>
                                                        item.type != 'KH' &&
                                                        item.type != 'VPKM'
                                                )
                                            "
                                            tableStyle="min-width: 50rem"
                                            scrollable
                                            resizableColumns
                                            columnResizeMode="fit"
                                            scrollHeight="500px"
                                        >
                                            <Column header="#">
                                                <template #body="sp">
                                                    <span>{{ sp.index + 1 }}</span>
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType !== 22"
                                                field="itemCode"
                                                header="Mã sản phẩm"
                                            >
                                            </Column>
                                            <Column
                                                field="itemName"
                                                header="Tên sản phẩm"
                                            >
                                                <template #body="{ data }">
                                                    <span
                                                        >{{ data.itemName }}
                                                        <Tag
                                                            v-if="data.typePromotion"
                                                            value="Giảm giá"
                                                        ></Tag>
                                                        <Tag
                                                            v-if="!data.price"
                                                            value="KM"
                                                        ></Tag
                                                    ></span>
                                                </template>
                                            </Column>
                                            <Column
                                                field="uomName"
                                                header="Đơn vị tính"
                                            ></Column>
                                            <Column
                                                header="Số lượng"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span>{{
                                                        formatNumber(sp.data.quantity)
                                                    }}</span>
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType !== 22"
                                                field="onHand"
                                                header="Lượng hàng gửi kho"
                                            >
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType === 22"
                                                :header="`Đơn giá (${currencySymbol.currency})`"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span>{{
                                                        formatNumber(sp.data.price)
                                                    }}</span>
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType === 22"
                                                header="Giảm giá (%)"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span>{{ sp.data.discount }}%</span>
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType === 22"
                                                :header="`Đơn giá sau giảm (${currencySymbol.currency})`"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span>{{
                                                        formatNumber(
                                                            sp.data.priceAfterDist
                                                        )
                                                    }}</span>
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType === 22"
                                                header="Thuế suất (%)"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span
                                                        >{{
                                                            formatNumber(sp.data.vat)
                                                        }}%</span
                                                    >
                                                </template>
                                            </Column>
                                            <Column
                                                v-if="DataProduct.objType === 22"
                                                header="Thành tiền trước thuế"
                                                style="text-align: end"
                                            >
                                                <template #body="sp">
                                                    <span>{{
                                                        formatNumber(
                                                            sp.data.priceAfterDist *
                                                                sp.data.quantity
                                                        )
                                                    }}</span>
                                                </template>
                                            </Column>
                                            <Column header="Phương thức thanh toán">
                                                <template #body="{ data }">
                                                    <span>{{
                                                        ptttLabel[data.paymentMethodCode]
                                                    }}</span>
                                                </template>
                                            </Column>
                                        </DataTable>
                                    </div>
                                    <div class="col-8">
                                        <DataTable
                                            v-if="DataProduct?.promotion?.length"
                                            scrollable
                                            scrollHeight="400px"
                                            showGridlines
                                            resizableColumns
                                            columnResizeMode="fit"
                                            :value="
                                                DataProduct.promotion?.filter(
                                                    (item) => item.quantityAdd > 0
                                                )
                                            "
                                            class="mb-4"
                                        >
                                            <template #header>
                                                <div class="font-bold text-lg">
                                                    Khuyến mãi
                                                </div>
                                            </template>
                                            <Column header="#" class="w-3rem">
                                                <template #body="{ index }">
                                                    {{ index + 1 }}
                                                </template>
                                            </Column>
                                            <Column
                                                field="itemCode"
                                                header="Mã sản phẩm"
                                            ></Column>
                                            <Column
                                                field="itemName"
                                                header="Tên hàng hóa"
                                            ></Column>
                                            <Column
                                                field="quantityAdd"
                                                style="text-align: end"
                                                header="Số lượng"
                                            ></Column>
                                            <Column
                                                field="packingName"
                                                header="Đơn vị tính"
                                            ></Column>
                                            <Column field="lineId" header="Dòng">
                                                <template #body="{ data }">
                                                    {{ data.lineId + 1 }}
                                                </template>
                                            </Column>
                                        </DataTable>
                                        <div
                                            class="card p-3 text-yellow-700 text-lg font-bold mb-4"
                                        >
                                            Tổng sản lượng
                                            {{
                                                formatNumber(
                                                    DataProduct?.itemDetail?.reduce(
                                                        (acc, item) =>
                                                            acc + item.numInSale,
                                                        0
                                                    )
                                                )
                                            }}
                                            lít
                                        </div>
                                        <div class="card flex flex-column gap-3 p-3">
                                            <div>
                                                <div class="font-semibold mb-2">
                                                    Ghi chú của khách hàng
                                                </div>
                                                <div
                                                    class="border-left-3 border-blue-200 p-3"
                                                    style="
                                                        background-color: rgb(
                                                            107 122 151 / 18%
                                                        );
                                                    "
                                                >
                                                    <p v-if="DataProduct.note">
                                                        {{ DataProduct.note }}
                                                    </p>
                                                    <p class="select-none" v-else>
                                                        &nbsp;
                                                    </p>
                                                </div>
                                            </div>
                                            <div>
                                                <label
                                                    for="sellerNote"
                                                    class="block font-semibold mb-2"
                                                >
                                                    Ghi chú của APSP
                                                </label>
                                                <div
                                                    v-if="!enableEditNote"
                                                    class="border-left-3 border-blue-200 p-3"
                                                    style="
                                                        background-color: rgb(
                                                            107 122 151 / 18%
                                                        );
                                                    "
                                                >
                                                    <p v-if="DataProduct.sellerNote">
                                                        {{ DataProduct.sellerNote }}
                                                    </p>
                                                    <p class="select-none" v-else>
                                                        &nbsp;
                                                    </p>
                                                </div>
                                                <!-- Chỉnh sửa ghi chú -->
                                                <Textarea
                                                    id="sellerNote"
                                                    v-else
                                                    rows="4"
                                                    v-model="sellerNote"
                                                    class="w-full"
                                                    placeholder="Nhập ghi chú đơn hàng"
                                                ></Textarea>
                                                <!-- Chỉnh sửa ghi chú -->
                                                <!-- v-if="DataProduct.status != 'DXN'" -->
                                                <div
                                                    v-if="
                                                        !['DXN', 'DHT'].includes(
                                                            DataProduct.status
                                                        )
                                                    "
                                                    class="flex justify-content-end mt-3"
                                                >
                                                    <!-- v-if="enableEditNote" -->
                                                    <div
                                                        v-if="enableEditNote"
                                                        class="flex gap-3"
                                                    >
                                                        <Button
                                                            @click="onClickCancelNote"
                                                            label="Hủy"
                                                            icon="pi pi-times"
                                                            severity="secondary"
                                                        >
                                                        </Button>
                                                        <Button
                                                            @click="onClickSaveNote"
                                                            label="Lưu"
                                                            icon="pi pi-save"
                                                            :loading="loadingNote"
                                                        />
                                                    </div>
                                                    <Button
                                                        v-else
                                                        @click="onClickEditNote"
                                                        label="Chỉnh sửa"
                                                        icon="pi pi-pencil"
                                                    />
                                                </div>
                                            </div>
                                        </div>

                                        <!-- Tài liệu đính kèm của đơn hàng -->
                                        <div
                                            v-if="DataProduct.attachFile?.length"
                                            class=""
                                        >
                                            <!-- {{ DataProduct.attachFile }} -->
                                            <DataTable
                                                :value="DataProduct.attachFile"
                                                showGridlines
                                            >
                                                <Column header="#" class="w-3rem">
                                                    <template #body="{ index }">
                                                        {{ index + 1 }}
                                                    </template>
                                                </Column>
                                                <Column
                                                    field="note"
                                                    header="Tên tài liệu"
                                                ></Column>
                                                <Column header="File đính kèm">
                                                    <template #body="{ data }">
                                                        <div
                                                            class="flex gap-2 align-items-end"
                                                            v-if="data.localFile"
                                                        >
                                                            <span>
                                                                {{
                                                                    data.localFile.name
                                                                }}</span
                                                            >
                                                        </div>
                                                        <span v-else>{{
                                                            data.fileName
                                                        }}</span>
                                                    </template>
                                                </Column>
                                                <Column
                                                    v-if="
                                                        !['DXN', 'DGH', 'DHT'].includes(
                                                            DataProduct.status
                                                        )
                                                    "
                                                    class="w-11rem text-center"
                                                >
                                                    <template #body="{ data, index }">
                                                        <FileUpload
                                                            mode="basic"
                                                            name="files[]"
                                                            withCredentials="hehe"
                                                            :auto="true"
                                                            :customUpload="true"
                                                            :maxFileSize="1000000"
                                                            @select="
                                                                onSeletDocs($event, index)
                                                            "
                                                            :chooseLabel="
                                                                data.localFile
                                                                    ? 'Thay đổi'
                                                                    : 'Chọn'
                                                            "
                                                        />
                                                    </template>
                                                </Column>
                                                <Column header="" class="w-3rem">
                                                    <template #body="{ data }">
                                                        <Button
                                                            :disabled="!data.filePath"
                                                            @click="
                                                                onClickDownloadFile(data)
                                                            "
                                                            icon="pi pi-download"
                                                            text
                                                        />
                                                    </template>
                                                </Column>
                                                <template #header>
                                                    <div
                                                        class="flex justify-content-between"
                                                    >
                                                        <div
                                                            class="font-bold text-lg my-2"
                                                        >
                                                            Tài liệu đính kèm
                                                        </div>
                                                        <div
                                                            v-if="
                                                                DataProduct.attachFile?.some(
                                                                    (row) => row.localFile
                                                                )
                                                            "
                                                            class="flex gap-2"
                                                        >
                                                            <Button
                                                                @click="onCancel"
                                                                label="Hủy"
                                                                severity="secondary"
                                                                :disabled="loadingDocs"
                                                            />
                                                            <Button
                                                                @click="onUploadDocs"
                                                                label="Lưu"
                                                                :loading="loadingDocs"
                                                            />
                                                        </div>
                                                    </div>
                                                </template>
                                            </DataTable>
                                        </div>
                                    </div>
                                    <div v-if="DataProduct.objType === 22" class="col-4">
                                        <BillDetail
                                            :paymentInfo="DataProduct.paymentInfo"
                                            :promotion="
                                                uniqBy(
                                                    DataProduct.promotion,
                                                    'promotionCode'
                                                )
                                            "
                                        ></BillDetail>
                                        <!-- <div class="border-1 border-200 p-3 h-full">
                                            <div class="flex flex-column gap-3">
                                                <div class="font-bold text-xl">
                                                    Thông tin thanh toán:
                                                </div>
                                                <hr class="my-0" />
                                                <div class="flex justify-content-between">
                                                    <span
                                                        >Thành tiền trước chiết khấu sản
                                                        lượng (vnđ):</span
                                                    >
                                                    <span
                                                        ><strong
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.totalBeforeVat ||
                                                                        0
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</strong
                                                        ></span
                                                    >
                                                </div>
                                                <div class="flex justify-content-between">
                                                    <span>Khuyến mại khác:</span>
                                                    <span
                                                        ><strong
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct.distcountAmount
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</strong
                                                        ></span
                                                    >
                                                </div>
                                                <div
                                                    v-if="
                                                        DataProduct?.paymentMethod[0]
                                                            ?.paymentMethodCode ==
                                                        'PayNow'
                                                    "
                                                    class="flex justify-content-between"
                                                >
                                                    <span
                                                        >Thưởng thanh toán ngay
                                                        (2%)<Button
                                                            size="small"
                                                            class="cursor-pointer p-0 m-0"
                                                            v-tooltip="{
                                                                value:
                                                                    'Giảm ngay 2% trên tổng giá niêm yết',
                                                                showDelay: 100,
                                                                hideDelay: 300,
                                                            }"
                                                            text
                                                            icon="pi pi-info-circle"
                                                        /
                                                        >:</span
                                                    >
                                                    <span
                                                        ><strong
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.bonusAmount
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</strong
                                                        ></span
                                                    >
                                                </div>
                                                <div class="flex justify-content-between">
                                                    <span>Tiền thuế (vnđ):</span>
                                                    <span
                                                        ><strong
                                                            >{{ formatNumber(totalVat) }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</strong
                                                        ></span
                                                    >
                                                </div>
                                                <div
                                                    v-const="
                                                        (thuongSL =
                                                            DataProduct.quarterlyCommitmentBonus ||
                                                            0 +
                                                                DataProduct.yearCommitmentBonus ||
                                                            0)
                                                    "
                                                >
                                                    <div v-if="thuongSL">
                                                        <hr class="mt-0 border-300" />
                                                        <div
                                                            class="flex justify-content-between"
                                                        >
                                                            <span>Thưởng tích lũy:</span>
                                                            <span
                                                                ><strong
                                                                    >{{
                                                                        formatNumber(
                                                                            thuongSL
                                                                        )
                                                                    }}
                                                                    {{
                                                                        currencySymbol.symbol
                                                                    }}</strong
                                                                ></span
                                                            >
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="py-3 border-y-1 border-300">
                                                    <div
                                                        class="flex justify-content-between mb-3"
                                                    >
                                                        <span>Thanh toán ngay:</span>
                                                        <span class="font-bold"
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.totalPayNow
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</span
                                                        >
                                                    </div>
                                                    <div
                                                        class="flex justify-content-between mb-3"
                                                    >
                                                        <span>Công nợ tín chấp:</span>
                                                        <span class="font-bold"
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.totalDebt
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</span
                                                        >
                                                    </div>
                                                    <div
                                                        class="flex justify-content-between"
                                                    >
                                                        <span>Công nợ bảo lãnh:</span>
                                                        <span class="font-bold"
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.totalDebtGuarantee
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</span
                                                        >
                                                    </div>
                                                </div>

                                                <div class="flex justify-content-between">
                                                    <span
                                                        >Tổng thanh toán ({{
                                                            currencySymbol.currency
                                                        }}):</span
                                                    >

                                                    <span
                                                        ><strong class="text-primary"
                                                            >{{
                                                                formatNumber(
                                                                    DataProduct
                                                                        ?.paymentInfo
                                                                        ?.totalAfterVat
                                                                )
                                                            }}
                                                            {{
                                                                currencySymbol.symbol
                                                            }}</strong
                                                        ></span
                                                    >
                                                </div>
                                                <div
                                                    v-if="0"
                                                    class="flex justify-content-between"
                                                >
                                                    <span>Phương thức thanh toán:</span>
                                                    <span
                                                        ><strong class="text-primary">{{
                                                            DataProduct.paymentMethod
                                                                ? DataProduct
                                                                      .paymentMethod[0]
                                                                      .paymentMethodName
                                                                : "--"
                                                        }}</strong></span
                                                    >
                                                </div>
                                            </div>
                                            <div
                                                class="flex flex-column align-items-center justify-content-center mt-3"
                                            >
                                                <div
                                                    v-if="
                                                        User?.id && User.currentCommited
                                                    "
                                                    class="w-full mb-2"
                                                >
                                                    <OutputCheck :customer="User">
                                                    </OutputCheck>
                                                </div>
                                            </div>
                                            <hr class="mt-0" />
                                            <div class="w-full">
                                                <DataTable
                                                    :value="
                                                        uniqBy(
                                                            DataProduct.promotion,
                                                            'promotionCode'
                                                        )
                                                    "
                                                    showGridlines
                                                >
                                                    <Column
                                                        field="promotionName"
                                                        header="Tên CTKM"
                                                    ></Column>
                                                    <Column
                                                        field="promotionDesc"
                                                        header="Mô tả"
                                                    ></Column>
                                                </DataTable>
                                            </div>
                                        </div> -->
                                    </div>
                                </div>
                            </div>
                            <template v-if="0">
                                <div v-if="!checkDebt" class="col-12">
                                    <DocumentsComp
                                        @callAPI="GetOrderDetail"
                                        :data="DataProduct"
                                        :idCheck="props.id"
                                    >
                                    </DocumentsComp>
                                </div>
                            </template>
                            <div class="col-12" v-if="DataProduct.status == 'DXN'">
                                <DataTable :value="chungChiGiaoHangs" showGridlines>
                                    <template #header>
                                        <div class="font-bold text-lg my-2">
                                            Chứng từ giao hàng
                                        </div>
                                    </template>
                                    <template #footer>
                                        <Button
                                            @click="onClickFilesChungTuGiaoHang"
                                            icon="pi pi-plus"
                                            label="Thêm tài liệu"
                                        />
                                        <input
                                            @change="onChangeFilesChungTuGiaoHang"
                                            ref="filesInput"
                                            type="file"
                                            class="hidden"
                                        />
                                    </template>
                                    <Column header="#" class="w-3rem">
                                        <template #body="sp">
                                            <span>{{ sp.index + 1 }}</span>
                                        </template>
                                    </Column>
                                    <Column header="Tệp đính kèm">
                                        <template #body="sp">
                                            {{ sp.data.name }}
                                        </template>
                                    </Column>
                                    <Column header="Người tạo" class="w-20rem"></Column>
                                    <Column header="Ngày tạo" class="w-20rem"></Column>
                                    <Column header="" class="w-3rem">
                                        <template #body="sp">
                                            <Button
                                                @click="
                                                    chungChiGiaoHangs.splice(sp.index, 1)
                                                "
                                                icon="pi pi-trash"
                                                severity="danger"
                                                text
                                            />
                                        </template>
                                    </Column>
                                    <template #empty>
                                        <div class="p-5 my-5 text-center">
                                            Vui lòng thêm tài liệu chứng từ giao hàng.
                                        </div>
                                    </template>
                                </DataTable>
                            </div>
                            <div
                                class="col-12"
                                v-if="['DGH', 'DHT'].includes(DataProduct.status)"
                            >
                                <DataTable
                                    :value="DataProduct.attDocuments || []"
                                    showGridlines
                                >
                                    <Column header="#" class="w-3rem">
                                        <template #body="sp">
                                            <span>{{ sp.index + 1 }}</span>
                                        </template>
                                    </Column>
                                    <Column header="Tệp đính kèm">
                                        <template #body="sp">
                                            <div>
                                                <span>
                                                    {{ sp.data.fileName }}
                                                </span>
                                            </div>
                                        </template>
                                    </Column>
                                    <Column
                                        field="authorName"
                                        header="Người tạo"
                                        class="w-20rem"
                                    ></Column>
                                    <Column header="Ngày tạo" class="w-10rem">
                                        <template #body="sp">
                                            {{
                                                format.DateTime(sp.data.uploadFileAt)
                                                    ?.date
                                            }}
                                        </template>
                                    </Column>
                                    <Column class="w-3rem">
                                        <template #body="sp">
                                            <Button
                                                :disabled="!sp.data.filePath"
                                                @click="onClickDownloadFile(sp.data)"
                                                icon="pi pi-download"
                                                text
                                            />
                                        </template>
                                    </Column>
                                    <template #empty>
                                        <div class="p-5 my-5 text-center">
                                            Vui lòng thêm tài liệu chứng từ giao hàng.
                                        </div>
                                    </template>
                                    <template #header>
                                        <div class="my-2 text-lg font-bold">
                                            Chứng từ giao hàng
                                        </div>
                                    </template>
                                </DataTable>
                            </div>
                        </div>
                    </div>
                    <hr class="m-0" />
                    <div class="flex gap-3 justify-content-end p-4">
                        <div
                            v-if="
                                isExceedDebt &&
                                !['DXN', 'DGH', 'DHT'].includes(DataProduct.status)
                            "
                            class="flex gap-2"
                        >
                            <template v-if="DataProduct.status == 'TTN'">
                                <Button
                                    @click="onClickHDH"
                                    label="Hủy đơn hàng"
                                    :loading="loadingHDH"
                                    severity="danger"
                                >
                                </Button>
                                <Button
                                    @click="onClickXNDH"
                                    label="Xác nhận đơn hàng"
                                    :loading="loadingXNDH"
                                >
                                </Button>
                            </template>
                            <template v-else>
                                <AdditionalRequest
                                    @onUpdate="GetOrderDetail()"
                                    :idCheck="props.id"
                                    :endpoint="`PurchaseOrder/${Route.params.id}`"
                                    :disabled="
                                        !(
                                            isExceedDebt &&
                                            DataProduct.approval?.status !== 'A'
                                        )
                                    "
                                >
                                </AdditionalRequest>
                                <Button
                                    :disabled="
                                        !DataProduct.attachFile?.length ||
                                        !DataProduct.attachFile?.every(
                                            (el) => el.filePath
                                        ) ||
                                        !(
                                            isExceedDebt &&
                                            DataProduct.approval?.status !== 'A'
                                        )
                                    "
                                    v-if="!props.id"
                                    @click="ApproveOrder(DataProduct.approval?.id)"
                                    label="Phê duyệt"
                                >
                                </Button>
                            </template>
                        </div>
                        <!-- checkDisabled() -->
                        <Button
                            v-if="
                                !['DXN', 'DGH', 'DHT', 'TTN'].includes(DataProduct.status)
                            "
                            :disabled="
                                isExceedDebt && DataProduct.approval?.status !== 'A'
                            "
                            @click="onClickConfirmOrder('DXN')"
                            label="Xác nhận"
                        />
                        <Button
                            :disabled="chungChiGiaoHangs.length < 1"
                            v-if="DataProduct.status == 'DXN'"
                            severity="info"
                            icon="pi pi-truck"
                            @click="onClickGiaoHang"
                            label="Giao hàng"
                        />
                        <Button
                            v-if="DataProduct.status == 'DGH'"
                            severity="primary"
                            icon="fa fa-solid fa-check"
                            @click="onClickHoanThanh"
                            label="Hoàn thành"
                        />
                    </div>
                </div>
                <div class="card p-0">
                    <div class="p-4 font-bold text-xl">Thông tin yêu cầu</div>
                    <hr class="m-0" />
                    <div class="grid p-4">
                        <div class="col-6">
                            <div class="card p-0 h-full">
                                <div class="p-3 font-bold">Thông tin khách hàng</div>
                                <hr class="m-0" />
                                <div class="flex flex-column gap-3 p-3">
                                    <div class="flex gap-3">
                                        <span>Tên khách hàng:</span
                                        ><strong>{{
                                            DataProduct.cardName
                                                ? DataProduct.cardName
                                                : "--"
                                        }}</strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Mã số thuế:</span
                                        ><strong>
                                            {{ User.licTradNum }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Số điện thoại:</span
                                        ><strong> {{ User.phone }} </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Email:</span
                                        ><strong>{{ User.email }} </strong>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="card p-0 h-full">
                                <div>
                                    <div class="font-bold p-3">Thông tin giao hàng</div>
                                </div>
                                <hr class="m-0" />
                                <div class="flex flex-column gap-3 p-3">
                                    <div class="flex gap-3">
                                        <span>Người liên hệ:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "S" &&
                                                        el.default === "Y"
                                                )[0]?.person
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Số điện thoại:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "B" &&
                                                        el.default === "Y"
                                                )[0]?.phone
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Căn cước công dân:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "S" &&
                                                        el.default === "Y"
                                                )[0]?.cccd
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Biển số xe:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "S" &&
                                                        el.default === "Y"
                                                )[0]?.vehiclePlate
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Địa chỉ giao hàng:</span
                                        ><strong>
                                            {{ userLocation(User, "S") }}
                                        </strong>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="card p-0 h-full">
                                <div class="p-3 font-bold">Thông tin xuất hóa đơn</div>
                                <hr class="m-0" />
                                <div class="flex flex-column gap-3 p-3">
                                    <div class="flex gap-3">
                                        <span>Người liên hệ:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "B" &&
                                                        el.default === "Y"
                                                )[0]?.person
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Số điện thoại:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "B" &&
                                                        el.default === "Y"
                                                )[0]?.phone
                                            }}
                                        </strong>
                                    </div>
                                    <div v-if="0" class="flex gap-3">
                                        <span>Căn cước công dân:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "B" &&
                                                        el.default === "Y"
                                                )[0]?.cccd
                                            }}
                                        </strong>
                                    </div>
                                    <div v-if="0" class="flex gap-3">
                                        <span>Biển số xe:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) =>
                                                        el.type == "B" &&
                                                        el.default === "Y"
                                                )[0]?.vehiclePlate
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>Địa chỉ giao hàng:</span
                                        ><strong>
                                            {{ userLocation(User, "B") }}
                                        </strong>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <DebtCheck
        :DebtData="User?.crD4"
        :TotalDebtData="User?.crD3"
        type="icon"
        :paymentMethod="DataProduct.paymentMethod?.[0]?.paymentMethodCode"
        :TotalPayment="0"
        :ItemDebt="null"
        @checkDebt="checkDebt = $event"
        style="visibility: hidden"
    >
    </DebtCheck>
    <ConfirmDialog></ConfirmDialog>
</template>
