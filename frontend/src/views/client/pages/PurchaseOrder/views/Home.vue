<script setup>
import { ref, computed, onBeforeMount } from "vue";
import API from "@/api/api-main";
import { useRouter, useRoute } from "vue-router";
import AddProducts from "@/components/order/AddProducts.vue";
import PaymentOrder from "@/components/order/PaymentOrder.vue"; 
import { useMeStore } from "@/Pinia/me";
 
const AddNewReqModal = ref(false);
const loading = ref(false);
const router = useRouter();
const route = useRoute();
const emit = defineEmits(["complete"]);
const User = ref({});
const selectedProduct = ref([]);
const payload = ref();

const isUserInfoLoaded = ref(false); 
const meStore = useMeStore();
onBeforeMount(async () => {
    // GetUser();
    const medata = await meStore.getMe();

    User.value = medata?.user?.bpInfo;
    if (User.value) {
        isUserInfoLoaded.value = true;
    }
    const copiedProducts = route.query.copiedProducts;
    if (copiedProducts) {
        setTimeout(() => {
            selectedProduct.value = JSON.parse(copiedProducts);
        }, 1000);
    }
    const isCart = route.query.isCart;
    const orderNow = route.query.itemId;
    if (isCart == "T") {
        getDataFromCart();
    }
    if (orderNow) {
        getDataFromOrderNow(orderNow);
    }
});

const getDataFromOrderNow = async (id) => {
    try {
        const { data } = await API.get(`Item/${id}`); 
        const res = {
            id: data.id,
            itemId: data.id,
            itemCode: data.itemCode,
            itemName: data.itemName,
            quantity: route.query.quantity ? route.query.quantity : 1,
            price: data.price,
            taxGroups: data.taxGroups || { rate: 0 },
            ougp: {
                ouom: {
                    uomName: data.ougp.ugpName,
                },
            },
            packing: {
                volumn: data.packing.volumn,
            },
            paymentMethodCode: "PayNow",
        };
        selectedProduct.value.push(res);

        
        // getOrderStats = res

    } catch (error) {
        console.error(error);
    }
};
const getDataFromCart = () => {
    // const tmp = JSON.parse(localStorage.getItem("cart"));

    // cart.forEach((item) => {
    //     selectedProduct.value.push(item);
    // });
    const prdctIds = route.query.prdctIds;
    const productCartIds = prdctIds.split(",").map((pid) => parseInt(pid));
    API.get("cart/me")
        .then((res) => {
            cartItemIds.push(...res.data?.items?.map((itm) => itm.id));
            const data = res.data.items?.map((itm) => {
                const _item = { ...itm.item }; 
                _item.quantity = itm.quantity;
                _item.price = itm.price;
                _item.discount = 0;
                _item.priceType = "P";
                _item.discountType = "P";
                _item.paymentMethodCode = itm.paymentMethodCode;
                return _item;
            });
            if (productCartIds?.length > 0 && false) {
                data.filter((prd) => productCartIds.includes(prd.id)).forEach((item) => {
                    selectedProduct.value.push(item);
                });
            } else {
                data.forEach((item) => {
                    selectedProduct.value.push(item);
                });
            }
        })
        .catch((error) => {
            console.error(error);
        });
};

// Khởi tạo dữ liệu
const InitData = () => {
    AddNewReqModal.value = false;
    payload.value = {
        id: 0,
        invoiceCode: "",
        cardId: 0,
        cardCode: "",
        cardName: "",
        docDate: new Date(),
        DeliveryTime: new Date(),
        discount: 0,
        distcountAmount: 0,
        vatAmount: 0,
        total: 0,
        note: "",
        userId: 1,
        status: "",
        itemDetail: [],
        address: [],
        paymentMethod: [
            {
                paymentMethodID: 1,
                paymentMethodCode: "PayNow",
                paymentMethodName: "Thanh toán ngay",
            },
        ],
    };
    selectedProduct.value = [];
};

InitData();

// Lấy thông tin người dùng
// const GetUser = () => {
//     API.get(`Account/me`).then((res) => {
//         if (res.data) User.value = res.data?.user?.bpInfo;
//     });
// };

const paymentMethodCode = computed(
    () => payload.value?.paymentMethod[0].paymentMethodCode
);
// END
const resetData = (event) => {
    InitData();
    emit("complete", true);
    router.replace(`/client/success?type=order&id=${event.id}`);
};

const cartItemIds = [];
const onClearCart = () => {
    API.delete(`Cart/me/bulk`, cartItemIds);
};
const AddProductAction = ref();
const outputCheckComp = ref();
</script>
<template>
    <div class="grid product_table mt-3">
        <div class="col-9 flex flex-column gap-3 card p-0 overflow-hidden mb-0">
            <TabView>
                <TabPanel header="Sản phẩm">
                    <AddProducts ref="AddProductAction" v-model:selectedProduct="selectedProduct" :Customer="User"
                        :paymentMethodCode="paymentMethodCode" :payload="payload" :isClient="true"></AddProducts>
                </TabPanel>
                <TabPanel header="Thông tin giao hàng">
                    <div class="card">
                        <div class="p-fluid">
                            <div class="grid align-items-end">
                                <div class="col-6">
                                    <div class="field">
                                        <label>Công ty(Optional)</label>
                                        <InputText readonly :value="typeof User === 'object'
                                            ? User?.cardName
                                            : ''
                                            " />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="field">
                                        <label>Tên khác</label>
                                        <InputText readonly :value="typeof User === 'object'
                                            ? User?.frgnName
                                            : ''
                                            " />
                                    </div>
                                </div>
                            </div>
                            <div class="grid align-items-end">
                                <div class="col-6">
                                    <div class="field">
                                        <label>Email</label>
                                        <InputText readonly :value="typeof User === 'object'
                                            ? User?.email
                                            : ''
                                            " />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="field">
                                        <label>Số điện thoại</label>
                                        <InputText readonly :value="typeof User === 'object'
                                            ? User?.phone
                                            : ''
                                            " />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="field">
                                        <label for="">Thời gian lấy hàng</label>
                                        <Calendar showTime showTimeInput hourFormat="12" v-model="payload.docDate"
                                            showIcon :minDate="new Date()" iconDisplay="input" dateFormat="dd/mm/yy"
                                            placeholder="Chọn thời gian">
                                        </Calendar>
                                    </div>
                                </div>
                            </div>
                            <div class="grid align-items-end">
                                <div class="col-12">
                                    <SearchAddress :customer="User" :data="User ? User.crD1 : null"></SearchAddress>
                                </div>
                            </div>
                        </div>
                    </div>
                </TabPanel>
            </TabView>
        </div>
        <div class="col-3 flex flex-column gap-3 py-0">
            <PaymentOrder v-model:selectedProduct="selectedProduct" :payload="payload" :Customer="User"
                @AddOrder="resetData($event)" @clear-cart="onClearCart" :isClient="true"
                :isUserInfoLoaded="isUserInfoLoaded" :isPromotionLoaded="AddProductAction?.isPromotionLoaded"
                :tienGiamSanLuong="outputCheckComp?.tiemGiam"></PaymentOrder>
            <!-- {{ User }} -->
            <!-- {{ selectedProduct }} -->
            <div v-if="User" class="w-full mb-2">
                <OutputCheck ref="outputCheckComp" :productsSelected="selectedProduct" :customer="User">
                </OutputCheck>
            </div>
        </div>
    </div>
    <Loading v-if="loading"></Loading>
</template>
<style scoped>
@media only screen and (max-width: 640px) {
    .product_table {
        display: flex;
        flex-direction: column;
        gap: 2rem;
    }

    .col-9,
    .col-3 {
        width: 100%;
    }

    .col-3 {
        order: 2;
    }
}

/* Màn hình di động lớn / máy tính bảng nhỏ (641px - 1023px) */
@media only screen and (min-width: 641px) and (max-width: 1023px) {
    .product_table {
        display: flex;
        flex-direction: column;
        gap: 2rem;
    }

    .col-9,
    .col-3 {
        width: 100%;
    }

    .col-3 {
        order: 2;
    }
}

/* Máy tính bảng hoặc màn hình nhỏ (≥ 1024px) */
@media only screen and (min-width: 1024px) and (max-width: 1279px) {
    .col-9 {
        width: 65%;
        /* Điều chỉnh kích thước cột */
    }

    .col-3 {
        width: 35%;
    }
}

/* Màn hình laptop (≥ 1280px) */
@media only screen and (min-width: 1280px) and (max-width: 1535px) {}

/* Màn hình lớn (≥ 1536px) */
@media only screen and (min-width: 1536px) and (max-width: 1920px) {}

/* Màn hình rất lớn (≥ 1921px) */
@media only screen and (min-width: 1921px) and (max-width: 2560px) {}

/* Màn hình 4K (≥ 2561px) */
@media only screen and (min-width: 2561px) {}

:deep(.p-dropdown.p-component.p-inputwrapper) {
    width: 100%;
}

:deep(.p-autocomplete-input.p-inputtext.p-component) {
    width: 100%;
}

:deep(.p-inputtext.p-component.p-inputnumber-input) {
    width: 100%;
}

.sticky-table {
    position: sticky;
    top: 5px;
    z-index: 10;
    padding: 10px;
}
</style>
