<script setup>
import { ref, onBeforeMount, computed, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import API from "@/api/api-main";
import format from "../../../../../helpers/format.helper";
import { filter } from "lodash";
import { useGlobal } from "@/services/useGlobal";
import { uniqBy } from "lodash";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();


const { toast, FunctionGlobal } = useGlobal();
const User = ref({});
const isLoading = ref(false);
const Route = useRoute();
const Router = useRouter();
const discardModal = ref(false);

const enableEditNote = ref(false);
const note = ref("");

const onClickHoanThanh = () => {
    API.get(`PurchaseOrder/${Route.params.id}/change-status/DHT`)
        .then((res) => {
            if (res.status == 200) {
                FunctionGlobal.$notify("S", "Đã hoàn thành đơn hàng!", toast);
                GetOrderDetail();
            }
        })
        .catch((error) => {
            FunctionGlobal.$notify("E", "Đã có lỗi xảy ra!", toast);
            console.error(error);
        });
};

const onClickCancelNote = () => {
    enableEditNote.value = false;
    note.value = "";
};
const onClickEditNote = () => {
    enableEditNote.value = true;
    note.value = DataProduct.value.note;
};
const onClickSaveNote = async () => {
    try {
        const res = await API.patch(`PurchaseOrder/${Route.params.id}`, {
            note: note.value?.trim(),
        });
        FunctionGlobal.$notify("S", "Cập nhật ghi chú thành công!", toast);
        DataProduct.value.note = note.value?.trim();
        enableEditNote.value = false;
    } catch (error) {
        FunctionGlobal.$notify("E", "Đã có lỗi xảy ra!", toast);
        console.error(error);
    }
};

const TimelineData = ref([
    {
        body: {
            title: "Đang xử lý",
            description: "Đơn hàng đang được APSP xử lý",
        },
    },
    {
        body: {
            title: "Tạo đơn đặt hàng",
            description: "Công ty cổ phần công nghệ FOXAI đã tạo đơn đặt hàng",
        },
    },
]);
const DataProduct = ref({});
// Route.params.id

onBeforeMount(() => {
    GetOrderDetail();
    GetUser();
});
const openDiscardDialog = () => {
    discardModal.value = true;
};
const GetOrderDetail = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`PurchaseOrder/${Route.params.id}`);
        if (res.data) DataProduct.value = res.data.item;
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == "NaN") return 0;
    return Intl.NumberFormat().format(num);
};

const totalVat = computed(() => {
    if (DataProduct.value.itemDetail) {
        return DataProduct.value.itemDetail.reduce((total, val) => {
            return total + val.vatAmount;
        }, 0);
    }
});
const checkAddressDeli = (type) => {
    if (DataProduct.value != {}) {
        if (DataProduct.value.address == undefined || !DataProduct.value.address.length)
            return "--";
        const addrString = DataProduct.value.address.filter((val) => {
            return val.type == type;
        })[0];
        return `${addrString.address}, ${addrString.locationName}, ${addrString.areaName}`;
    }
    return "--";
};
import { useMeStore } from "../../../../../Pinia/me";
const meData = useMeStore();
const GetUser = async () => {
    const me = await meData.getMe();
    User.value = me?.user?.bpInfo;
    // try {
    //     const res = await API.get(`Account/me`);
    //     if (res.data) User.value = res.data.user.bpInfo;
    // } catch (error) {}
};

const userLocation = (user, type) => {
    if (!user.crD1 || user.crD1.length === 0) {
        return "";
    }
    const locationData = user.crD1.find((el) => el.type === type);
    return locationData ? `${locationData.locationName} - ${locationData.areaName}` : "";
};

let stt = {
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
        label: t('body.status.HUY'),
    },
    HUY2: {
        class: "text-red-700 bg-red-200",
        label: t('body.status.HUY2'),
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
    return stt[data] || { class: "text-yellow-500", label: data };
};
watch(
    () => Route.params.id,
    (newVal, oldVal) => {
        if (newVal !== oldVal) {
            GetOrderDetail();
            GetUser();
        }
    },
    { immediate: true }
);
const formData = new FormData();

const DiscardOrder = async () => {
    try {
        formData.set("document", JSON.stringify({ ...DataProduct.value, status: "HUY" }));
        const res = await API.update(`PurchaseOrder/${Route.params.id}`, formData);
        if (res.data) {
            FunctionGlobal.$notify("S", "Huỷ đơn hàng thành công!", toast);
            formData.delete("document");
        }
    } catch (error) {
        console.error(error);
    } finally {
        discardModal.value = false;
    }
};
const copyOrder = () => {
    const productsToCopy = DataProduct.value.itemDetail.filter(
        (el) => el.type !== "KH" && el.type !== "VPKH"
    );
    Router.push({
        path: "/client/order/new",
        query: { copiedProducts: JSON.stringify(productsToCopy) },
    });
};

const ptttLabel = ref({
    PayNow: "Thanh toán ngay",
    PayCredit: "Công nợ tín chấp",
    PayCredit: "Công nợ bảo lãnh",
});

const loadingPay = ref(false);
const onClickPay = () => {
    const payData = {
        docId: DataProduct.value.id,
        paymentAmount: DataProduct.value.paymentInfo?.totalPayNow + "00",
        paymentMethodId: 0,
    };
    loadingPay.value = true;

    API.add(`payment`, payData)
        .then((res) => {
            const redirectLink = res.data.redirectLink;
            if (redirectLink) {
                // Chuyển hướng one payment

                window.open(redirectLink, "_self");
            }
        })
        .catch((error) => {})
        .finally(() => {
            loadingPay.value = false;
        });
};
</script>
<template>
    <div v-if="DataProduct">
        <div class="grid align-items-center">
            <div class="col-12">
                <div class="flex gap-2 justify-content-between">
                    <h4 class="m-0 font-bold">
                        Chi tiết đơn hàng - {{ DataProduct.invoiceCode }}
                    </h4>
                    <div class="flex gap-2">
                        <Button
                            @click="openDiscardDialog()"
                            v-if="DataProduct.status == '' || DataProduct.status == 'DXL'"
                            v-tooltip.bottom="'Huỷ đơn hàng'"
                            outlined
                            icon="p-button-icon pi pi-ban"
                            raised
                        />
                        <Button
                            v-tooltip.bottom="'Sao chép đơn'"
                            outlined
                            icon="pi pi-clone"
                            raised
                            @click="copyOrder"
                        />
                    </div>
                </div>
            </div>
        </div>
        <div class="grid">
            <div class="col-12">
                <div class="card">
                    <div class="grid justify-content-between">
                        <div class="col-6">
                            <div class="flex flex-column gap-3">
                                <div class="flex gap-3">
                                    <span>Mã đơn hàng:</span
                                    ><strong>{{ DataProduct.invoiceCode }}</strong>
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
                                    <span>Thời gian nhận:</span
                                    ><strong>{{
                                        DataProduct.deliveryTime
                                            ? format.DateTime(DataProduct.deliveryTime)
                                                  .time +
                                              " " +
                                              format.DateTime(DataProduct.deliveryTime)
                                                  .date
                                            : "--"
                                    }}</strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>Trạng thái đơn hàng:</span
                                    ><Tag
                                        :class="formatStatus(DataProduct.status).class"
                                        >{{ formatStatus(DataProduct.status).label }}</Tag
                                    >
                                </div>
                                <!-- <div class="flex gap-3">
                                    <span>Trạng thái thanh toán:</span>
                                    <span>---</span>
                                </div> -->

                                <!-- <div class="flex gap-3">
                                    <span>Người liên hệ:</span><span>--</span>
                                </div> -->
                            </div>
                        </div>
                        <div class="col-6 flex justify-content-end">
                            <div class="w-17rem flex justify-content-end">
                                <!-- <DebtCheck
                                    :DebtData="User?.crD4"
                                    :TotalDebtData="User?.crD3"
                                    type="icon"
                                    :paymentMethod="
                                        DataProduct.paymentMethod?.[0]?.paymentMethodCode
                                    "
                                    :TotalPayment="0"
                                    :ItemDebt="null"
                                >
                                </DebtCheck> -->
                                <div>
                                    <KiemTraCongNo
                                        :bpId="User?.id"
                                        :bpName="User?.cardName"
                                        :payCredit="DataProduct?.paymentInfo?.totalDebt"
                                        :payGuarantee="
                                            DataProduct?.paymentInfo?.totalDebtGuarantee
                                        "
                                    />
                                </div>
                            </div>
                        </div>

                        <div class="col-12">
                            <DataTable
                                showGridlines
                                :value="
                                    DataProduct.itemDetail?.filter(
                                        (el) => el.type !== 'KH' && el.type !== 'VPKH'
                                    )
                                "
                                tableStyle="min-width: 50rem"
                                scrollable
                                scrollHeight="500px"
                                resizableColumns
                                columnResizeMode="fit"
                            >
                                <Column header="#">
                                    <template #body="sp">
                                        <span>{{ sp.index + 1 }}</span>
                                    </template>
                                </Column>
                                <Column field="itemName" header="Tên hàng hóa"></Column>
                                <Column
                                    header="Số lượng"
                                    style="min-width: 100px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{ formatNumber(sp.data.quantity) }}</span>
                                    </template>
                                </Column>
                                <Column field="uomName" header="Đơn vị tính"></Column>
                                <Column
                                    header="Đơn giá (VNĐ)"
                                    style="min-width: 130px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{ formatNumber(sp.data.price) }}</span>
                                    </template>
                                </Column>
                                <Column
                                    header="Giảm giá (%)"
                                    style="min-width: 130px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{ sp.data.discount }}%</span>
                                    </template>
                                </Column>
                                <Column
                                    header="Đơn giá sau giảm (VNĐ)"
                                    style="min-width: 190px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{
                                            formatNumber(sp.data.priceAfterDist)
                                        }}</span>
                                    </template>
                                </Column>
                                <Column
                                    header="Thuế suất (%)"
                                    style="min-width: 130px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{ formatNumber(sp.data.vatCode) }}%</span>
                                    </template>
                                </Column>
                                <Column
                                    header="Thành tiền trước thuế (VNĐ)"
                                    style="min-width: 200px; text-align: end"
                                >
                                    <template #body="sp">
                                        <span>{{
                                            formatNumber(
                                                sp.data.priceAfterDist * sp.data.quantity
                                            )
                                        }}</span>
                                    </template>
                                </Column>
                                <Column header="Phương thức thanh toán">
                                    <template #body="sp">
                                        <span>{{
                                            ptttLabel[sp.data.paymentMethodCode]
                                        }}</span>
                                    </template>
                                </Column>
                            </DataTable>
                        </div>
                        <div class="col-8">
                            <DataTable
                                showGridlines
                                :value="DataProduct.promotion"
                                scrollable
                                scrollHeight="400px"
                                resizableColumns
                                columnResizeMode="fit"
                            >
                                <template #header>
                                    <strong>{{ t('Navbar.menu.promotion') }}</strong>
                                </template>
                                <Column header="#">
                                    <template #body="sp">
                                        <div>
                                            {{ sp.index + 1 }}
                                        </div>
                                    </template>
                                </Column>
                                <Column field="itemCode" :header="t('body.report.table_header_product_code_3')"></Column>
                                <Column field="itemName" :header="t('body.report.table_header_product_name_3')"></Column>
                                <Column
                                    field="quantityAdd"
                                    style="text-align: end"
                                    :header="t('client.quantity')"
                                ></Column>
                                <Column field="packingName" header="ĐVT"></Column>
                                <Column field="lineId" header="Dòng">
                                    <template #body="{ data }">
                                        {{ data.lineId + 1 }}
                                    </template>
                                </Column>
                            </DataTable>
                            <div class="card p-3 border-noround">
                                <strong class="text-yellow-700"
                                    >Tổng sản lượng:
                                    {{
                                        formatNumber(
                                            DataProduct?.itemDetail?.reduce(
                                                (acc, item) => acc + item.numInSale,
                                                0
                                            )
                                        )
                                    }}
                                    (Lít)</strong
                                >
                            </div>
                            <hr />
                            <div class="text-gray-500 mt-3 mb-2 font-bold">
                                Ghi chú đơn hàng
                            </div>
                            <div
                                class="border-left-3 border-blue-200 p-3"
                                style="background-color: rgb(107 122 151 / 18%)"
                            >
                                <p v-if="DataProduct.note">
                                    {{ DataProduct.note }}
                                </p>
                                <p class="select-none" v-else>&nbsp;</p>
                            </div>
                            <div class="text-gray-500 mt-3 mb-2 font-bold">
                                Ghi chú của APSP
                            </div>
                            <div
                                class="border-left-3 border-blue-200 p-3"
                                style="background-color: rgb(107 122 151 / 18%)"
                            >
                                <p v-if="DataProduct.sellerNote">
                                    {{ DataProduct.sellerNote }}
                                </p>
                                <p class="select-none" v-else>&nbsp;</p>
                            </div>
                        </div>
                        <div class="col-4">
                            <BillDetail
                                :paymentInfo="DataProduct.paymentInfo"
                                :promotion="DataProduct.promotion"
                                :showInfoButton="false"
                                :currency="DataProduct.currency"
                            />
                        </div>
                        <div class="col-12 pb-0">
                            <hr />
                            <div class="flex justify-content-end">
                                <Button
                                    :loading="loadingPay"
                                    @click="onClickPay"
                                    label="Thanh toán"
                                    v-if="['CTT'].includes(DataProduct.status)"
                                />
                                <Button
                                    @click="onClickHoanThanh"
                                    label="Hoàn thành"
                                    v-if="DataProduct.status == 'DGH'"
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-12">
                <DocumentsComp
                    @callAPI="GetOrderDetail"
                    :data="DataProduct"
                ></DocumentsComp>
            </div>
            <div class="col-12">
                <div class="card">
                    <div class="text-xl font-bold mb-0">{{t('Custom.request_info')}}</div>
                    <hr />
                    <div class="grid">
                        <div class="col-6 flex flex-column gap-4">
                            <div class="card m-0 flex flex-column gap-3">
                                <strong
                                    >{{t('client.customer_info')}}
                                    <hr />
                                </strong>
                                <div class="flex flex-column gap-3">
                                    <div class="flex gap-3">
                                        <span>{{t('client.customer_name')}}:</span
                                        ><strong>{{
                                            DataProduct.cardName
                                                ? DataProduct.cardName
                                                : "--"
                                        }}</strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.tax_code')}}:</span
                                        ><strong> {{ User.licTradNum }} </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.phone_number_label')}}</span
                                        ><strong> {{ User.phone }} </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.email_label')}}</span
                                        ><strong> {{ User.email }} </strong>
                                    </div>
                                </div>
                            </div>
                            <div class="card flex flex-column gap-3">
                                <div>
                                    <div
                                        class="flex justify-content-between align-items-center"
                                    >
                                        <strong>{{t('client.invoice_info')}}</strong>
                                    </div>
                                    <hr />
                                </div>
                                <div class="flex flex-column gap-3">
                                    <div class="flex gap-3">
                                        <span>{{t('client.contact_person')}}:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) => el.type == "S"
                                                )[0]?.person
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.phone_number')}}:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) => el.type == "S"
                                                )[0]?.phone
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{ t('client.delivery_address') }}:</span
                                        ><strong> {{ userLocation(User, "S") }} </strong>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-6">
                            <!-- <div class="card">
                <strong>Lịch sử hoạt động
                  <hr />
                </strong>
                <StepHistory :data="TimelineData"></StepHistory>
              </div> -->
                            <div class="card flex flex-column gap-3">
                                <div>
                                    <div
                                        class="flex justify-content-between align-items-center"
                                    >
                                        <strong>{{ t('client.edit_delivery_info') }}</strong>
                                    </div>
                                    <hr />
                                </div>
                                <div class="flex flex-column gap-3">
                                    <div class="flex gap-3">
                                        <span>{{ t('client.contact_person') }}:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) => el.type == "B"
                                                )[0]?.person
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{[t('client.invoice_phone')]}:</span
                                        ><strong>
                                            {{
                                                User.crD1?.filter(
                                                    (el) => el.type == "B"
                                                )[0]?.phone
                                            }}
                                        </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.pickup_time')}}:</span
                                        ><strong> -- </strong>
                                    </div>
                                    <div class="flex gap-3">
                                        <span>{{t('client.delivery_address')}}:</span
                                        ><strong> {{ userLocation(User, "B") }} </strong>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <Dialog
        v-model:visible="discardModal"
        modal
        header="Xác nhận"
        :style="{ width: '40rem' }"
    >
        <div class="flex align-items-center p-2">
            <strong>t('Custom.confirm_cancel_oder') ?</strong>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    :label="t('Logout.cancel')"
                    @click="discardModal = false"
                    severity="secondary"
                />
                <Button
                    label="Xác nhận"
                    severity="danger"
                    @click="DiscardOrder()"
                />
            </div>
        </template>
    </Dialog>
    <loading v-if="isLoading"></loading>
</template>
