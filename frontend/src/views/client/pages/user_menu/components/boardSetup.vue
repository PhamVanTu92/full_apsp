<template>
    <div>
        <div class="mb-3">
            <p class="text-4xl font-semibold">{{ t('client.hello') }}, {{ username }}</p>
            <p class="">
               {{ t('client.dashboard_description')}}
                <router-link
                    :to="{ name: 'hisPur' }"
                    class="font-semibold text-primary underline link"
                    >{{t('client.purchase_history')}}</router-link
                >. {{t('client.manage_delivery_account')}}
                <router-link
                    :to="{ name: 'user' }"
                    class="font-semibold text-primary underline link"
                    >{{t('client.here')}}</router-link
                >

                <!-- quản lý
                <router-link :to="{ name: 'user' }" class="font-semibold text-green-500"
                    >Địa chỉ giao hàng.
                </router-link> -->
            </p>
        </div>
        <div class="card mt-3" v-if="0">
            <div class="flex justify-content-between align-items-center mb-5">
                <p class="font-semibold uppercase">Ưu đãi hấp dẫn</p>
                <Button
                    label="Xem tất cả"
                    icon="pi pi-arrow-right"
                    iconPos="right"
                    severity="warning"
                    text
                />
            </div>
            <div class="flex mb-2 justify-content-between align-items-center">
                <Avatar label="P" size="large" shape="circle" />
                <div>
                    <strong>50.000.000/100.000.000</strong>
                </div>
                <Avatar image="/image/gift.png" size="large" />
            </div>
            <ProgressBar :value="50"></ProgressBar>
            <div class="text-center mt-4">
                <h2 class="uppercase m-0">Chúc mừng bạn</h2>
                <p class="m-0">
                    Chỉ còn 50.000.000 nữa bạn sẽ nhận được Voucher hoàn tiền hấp dẫn
                </p>
            </div>
        </div>
        <!-- Thong tin dia chi -->
        <div v-if="0" class="mt-3 grid justify-content-around">
            <div class="lg:col-4 md:col-6 col-12 bg-white">
                <div
                    class="border-1 border-solid border-gray-200 border-round-md h-full flex flex-column"
                >
                    <div class="border-bottom-1 border-gray-200">
                        <div class="p-3 font-semibold uppercase">
                            Thông tin liên hệ mặc định
                        </div>
                    </div>

                    <div
                        class="p-3 flex flex-column gap-3 flex-grow-1 justify-content-between"
                    >
                        <!-- {{ contact }} -->
                        <div v-if="contact" class="flex gap-3">
                            <Avatar
                                :label="contact.person[0]"
                                shape="circle"
                                size="xlarge"
                            ></Avatar>
                            <div class="flex justify-content-center flex-column gap-2">
                                <div class="font-bold text-xl">
                                    {{ contact.person }}
                                </div>
                                <div class="text-gray-600">
                                    <span class="font-semibold">SĐT:</span>
                                    <span class="ml-2">{{ contact.phone }}</span>
                                </div>
                                <div class="text-gray-600">
                                    <span class="font-semibold">SĐT khác:</span>
                                    <span class="ml-2">{{ contact.phone1 }}</span>
                                </div>
                                <div class="text-gray-600">
                                    <span class="font-semibold">Email:</span>
                                    <span class="ml-2">{{ contact.email }}</span>
                                </div>
                            </div>
                        </div>
                        <div class="my-5 mx-auto font-italic" v-else>
                            Chưa thiết lập thông tin liên hệ mặc định
                        </div>
                        <router-link to="/client/setup/user" class="block">
                            <Button
                                :label="t('client.editInfo')"
                                class="p-3 w-full"
                                outlined
                            />
                        </router-link>
                    </div>
                </div>
            </div>
            <div class="lg:col-4 md:col-6 col-12 bg-white">
                <div
                    class="border-1 border-solid border-gray-200 border-round-md h-full flex flex-column"
                >
                    <div class="border-bottom-1 border-gray-200">
                        <div class="p-3 font-semibold uppercase">
                            Địa chỉ giao hàng mặc định
                        </div>
                    </div>

                    <div
                        class="p-3 flex gap-3 flex-column flex-grow-1 justify-content-between"
                    >
                        <!-- {{ delivery }} -->
                        <div v-if="contact" class="flex flex-column gap-3">
                            <div class="flex gap-3">
                                <Avatar
                                    :label="delivery.person[0]"
                                    shape="circle"
                                    size="xlarge"
                                ></Avatar>
                                <div
                                    class="flex justify-content-center flex-column gap-2"
                                >
                                    <div class="font-bold text-xl">
                                        {{ delivery.person }}
                                    </div>
                                    <div v-if="0" class="text-gray-600">
                                        <span class="font-semibold">CCCD:</span>
                                        <span class="ml-2">{{ delivery.cccd }}</span>
                                    </div>
                                    <div class="text-gray-600">
                                        <span class="font-semibold">SĐT:</span>
                                        <span class="ml-2">{{ delivery.phone }}</span>
                                    </div>
                                    <div class="text-gray-600">
                                        <span class="font-semibold">Email:</span>
                                        <span class="ml-2">{{ delivery.email }}</span>
                                    </div>
                                </div>
                            </div>
                            <div class="flex flex-column gap-3">
                                <div class="text-gray-600">
                                    <span class="font-semibold">Đ/c:</span>
                                    <span class="ml-2">{{
                                        getAddressLabel(delivery)
                                    }}</span>
                                </div>
                            </div>
                        </div>
                        <div class="my-5 mx-auto font-italic" v-else>
                            Chưa thiết lập địa chỉ giao hàng mặc định
                        </div>
                        <router-link to="/client/setup/user" class="block">
                            <Button
                                :label="t('client.editInfo')"
                                class="p-3 w-full"
                                outlined
                            />
                        </router-link>
                    </div>
                </div>
            </div>
            <div
                class="lg:col-4 md:col-12 col-12 flex flex-column justify-content-between pr-0 py-0"
            >
                <div class="bg-blue-100 border-round-sm p-3">
                    <div class="flex align-items-center">
                        <i
                            class="fa-solid fa-rocket p-3 bg-white border-round-sm text-3xl mr-3 text-blue-300"
                        ></i>
                        <div class="flex flex-column justify-content-around">
                            <label for="" class="text-2xl font-bold">{{
                                dashboard.totalOrderInTheMonth
                            }}</label>
                            <label for=""
                                >Tổng số đơn hàng tháng
                                {{ new Date().getMonth() + 1 }}</label
                            >
                        </div>
                    </div>
                </div>
                <div class="bg-orange-100 border-round-sm p-3">
                    <div class="flex">
                        <i
                            class="fa-regular fa-newspaper p-3 bg-white border-round-sm text-3xl mr-3 text-orange-300"
                        ></i>
                        <div class="flex flex-column justify-content-around">
                            <label for="" class="text-2xl font-bold">05</label>
                            <label for="">Đơn hàng chưa hoàn tất</label>
                        </div>
                    </div>
                </div>
                <div class="bg-green-100 border-round-sm p-3">
                    <div class="flex">
                        <i
                            class="fa-solid fa-box-archive p-3 bg-white border-round-sm text-3xl mr-3 text-green-300"
                        ></i>
                        <div class="flex flex-column justify-content-around">
                            <label for="" class="text-2xl font-bold">149</label>
                            <label for="">Đơn hàng đã hoàn tất</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="grid mt-3 w-full">
            <div class="flex justify-content-between w-full">
                <div class="col-12 p-0">
                    <div class="flex justify-content-between">
                        <div class="col-4 pt-0">
                            <div
                                class="card p-0 flex flex-column justify-content-between h-full mb-0"
                            >
                                <h5 class="m-0 p-3 text-700">{{t('client.accountInfo')}}</h5>
                                <div
                                    class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3"
                                >
                                    <div>
                                        <span class="mr-2 font-semibold"
                                            >{{t('client.fullName')}}:</span
                                        >
                                        <span>
                                            {{ user.fullName }}
                                        </span>
                                    </div>
                                    <div>
                                        <span class="mr-2 font-semibold">{{t('client.email')}}:</span>
                                        <span>
                                            {{ user.email }}
                                        </span>
                                    </div>
                                    <div>
                                        <span class="mr-2 font-semibold"
                                            >{{t('client.phoneNumber')}}:</span
                                        >
                                        <span>
                                            {{ user?.phone }}
                                        </span>
                                    </div>
                                    <div>
                                        <span class="mr-2 font-semibold"
                                            >{{t('client.birthday')}}</span
                                        >
                                        <span v-if="user?.bpInfo?.dateOfBirth">
                                            {{
                                                format(
                                                    user?.bpInfo?.dateOfBirth,
                                                    "dd/MM/yyyy"
                                                )
                                            }}
                                        </span>
                                    </div>
                                    <div>
                                        <span class="mr-2 font-semibold">{{t('client.address')}}:</span>
                                        <span>{{
                                            [
                                                user.bpInfo?.address,
                                                user.bpInfo?.locationName,
                                                user.bpInfo?.areaName,
                                            ]
                                                .filter((item) => item)
                                                .join(", ")
                                        }}</span>
                                    </div>
                                </div>
                                <div class="p-3">
                                    <router-link to="/client/setup/user">
                                        <Button
                                           :label="t('client.editInfo')"
                                            class="p-3 w-full"
                                            outlined
                                        />
                                    </router-link>
                                </div>
                            </div>
                        </div>
                        <div class="col-4 pt-0">
                            <template>{{
                                (s_user = user?.bpInfo?.crD1?.find(
                                    (el) => el.default == "Y" && el.type == "S"
                                ))
                            }}</template>
                            <div
                                class="card p-0 flex flex-column justify-content-between h-full mb-0"
                            >
                                <h5 class="m-0 p-3 text-700">{{t('client.shippingAddress')}}</h5>
                                <div
                                    class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3"
                                >
                                    <template v-if="s_user">
                                        <div>
                                            <span class="mr-2 font-semibold"
                                                >{{t('client.fullName')}}:</span
                                            >
                                            <span>
                                                {{ s_user?.person }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                               {{t('client.citizenId')}}
                                            </span>
                                            <span>
                                                {{ s_user?.cccd }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                {{t('client.address')}}:
                                            </span>
                                            <span>
                                                {{ getAddress(s_user) }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                {{t('client.email')}}
                                            </span>
                                            <span>
                                                {{ s_user?.email }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                 {{t('client.phoneNumber')}}
                                            </span>
                                            <span>
                                                {{ s_user?.phone }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                {{t('client.licensePlate')}}
                                            </span>
                                            <span>
                                                {{ s_user?.vehiclePlate }}
                                            </span>
                                        </div>
                                    </template>
                                    <div v-else class="text-center text-gray-600 h-full">
                                        {{t('client.no_delivery_address')}}
                                    </div>
                                </div>
                                <div class="p-3">
                                    <router-link to="/client/setup/user">
                                        <Button
                                            :label="t('client.editInfo')"
                                            class="p-3 w-full"
                                            outlined
                                        />
                                    </router-link>
                                </div>
                            </div>
                        </div>
                        <div class="col-4 pt-0">
                            <template>{{
                                (b_user = user?.bpInfo?.crD1?.find(
                                    (el) => el.default == "Y" && el.type == "B"
                                ))
                            }}</template>
                            <div
                                class="card p-0 flex flex-column justify-content-between h-full mb-0"
                            >
                                <h5 class="m-0 p-3 text-700">{{t('client.billingAddress')}}</h5>
                                <div
                                    class="flex-grow-1 flex flex-column gap-3 border-y-1 border-200 p-3"
                                >
                                    <template v-if="b_user">
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                 {{t('client.fullName')}}
                                            </span>
                                            <span>
                                                {{ b_user?.person }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                  {{t('client.address')}}
                                            </span>
                                            <span>
                                                {{ getAddress(b_user) }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                                 {{t('client.email')}}
                                            </span>
                                            <span>
                                                {{ b_user?.email }}
                                            </span>
                                        </div>
                                        <div>
                                            <span class="mr-2 font-semibold">
                                               {{t('client.phoneNumber')}}
                                            </span>
                                            <span>
                                                {{ b_user?.phone }}
                                            </span>
                                        </div>
                                    </template>
                                    <div v-else class="text-center text-gray-600 h-full">
                                       {{t('client.no_delivery_address')}}
                                    </div>
                                </div>
                                <div class="p-3">
                                    <router-link to="/client/setup/user">
                                        <Button
                                            :label="t('client.editInfo')"
                                            class="p-3 w-full"
                                            outlined
                                        />
                                    </router-link>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- <div></div> -->
            </div>
            <div class="w-full">
                <OrderStats></OrderStats>
            </div>
            <div v-if="0" class="w-full">
                <div class="flex mx-5 justify-content-between align-items-center">
                    <div class="text-2xl font-bold">{{t('client.orders')}}</div>
                    <router-link :to="{ name: 'hisPur' }">
                        <div
                            class="text-right text-blue-500 hover:underline cursor-pointer"
                        >
                            {{t('client.viewDetails')}}
                        </div>
                    </router-link>
                </div>
                <div class="p-3 flex flex-row justify-content-between w-full pr-0">
                    <div class="col-3">
                        <router-link :to="{ name: 'hisPur' }">
                            <div
                                class="bg-blue-100 border-round-sm p-3 hover:bg-blue-200"
                            >
                                <div class="flex">
                                    <i
                                        class="fa-regular fa-newspaper p-3 bg-white border-round-sm text-3xl mr-3 text-blue-300"
                                    ></i>
                                    <div class="flex flex-column justify-content-around">
                                        <div for="" class="text-2xl font-bold">05</div>
                                        <div for="">Đang xử lý</div>
                                    </div>
                                </div>
                            </div>
                        </router-link>
                    </div>
                    <div class="col-3">
                        <router-link :to="{ name: 'hisPur' }">
                            <div
                                class="bg-orange-100 border-round-sm p-3 hover:bg-orange-200 cursor-pointer"
                            >
                                <div class="flex">
                                    <i
                                        class="fa-regular fa-newspaper p-3 bg-white border-round-sm text-3xl mr-3 text-orange-300"
                                    ></i>
                                    <div class="flex flex-column justify-content-around">
                                        <div for="" class="text-2xl font-bold">05</div>
                                        <div for="">Đã xử lý</div>
                                    </div>
                                </div>
                            </div>
                        </router-link>
                    </div>
                    <div class="col-3">
                        <router-link :to="{ name: 'hisPur' }">
                            <div
                                class="bg-yellow-100 border-round-sm p-3 hover:bg-yellow-200"
                            >
                                <div class="flex">
                                    <i
                                        class="fa-regular fa-newspaper p-3 bg-white border-round-sm text-3xl mr-3 text-yellow-300"
                                    ></i>
                                    <div class="flex flex-column justify-content-around">
                                        <div class="text-2xl font-bold">05</div>
                                        <div>Giao hàng</div>
                                    </div>
                                </div>
                            </div>
                        </router-link>
                    </div>
                    <div class="col-3">
                        <router-link :to="{ name: 'hisPur' }">
                            <div
                                class="bg-green-100 border-round-sm p-3 hover:bg-green-200"
                            >
                                <div class="flex">
                                    <i
                                        class="fa-regular fa-newspaper p-3 bg-white border-round-sm text-3xl mr-3 text-green-300"
                                    ></i>
                                    <div class="flex flex-column justify-content-around">
                                        <div class="text-2xl font-bold">05</div>
                                        <div>Hoàn thành</div>
                                    </div>
                                </div>
                            </div>
                        </router-link>
                    </div>
                </div>
            </div>
        </div>
        <!-- Thong tin dia chi -->
        <div
            v-if="0"
            class="border-1 border-round-md border-solid border-gray-200 mt-4 bg-white"
        >
            <div class="font-medium border-bottom-1 border-gray-200 bg-white">
                <div class="mx-3 mt-4 flex justify-content-between">
                    <p class="">LỊCH SỬ MUA HÀNG</p>
                    <a href="" class="text-orange-400 hover:underline"
                        >Xem tất cả <i class="fa-solid fa-arrow-right ml-2"></i
                    ></a>
                </div>
            </div>

            <div>
                <DataTable
                    :value="products"
                    header="surface-200"
                    :rowStyleClass="getRowClass"
                    tableStyle="min-width: 50rem"
                >
                    <Column
                        field="productCode"
                        header="MÃ ĐƠN HÀNG"
                        style="font-size: 14px"
                    ></Column>
                    <Column field="status" header="TRẠNG THÁI" style="font-size: 14px">
                        <template #body="slotProps">
                            <div
                                class="text-sm font-medium"
                                :class="getSeverity(slotProps.data.status)"
                            >
                                {{ slotProps.data.status }}
                            </div>
                        </template>
                    </Column>
                    <Column
                        field="orderDate"
                        header="NGÀY ĐẶT HÀNG"
                        style="font-size: 14px"
                    >
                    </Column>
                    <Column
                        field="total"
                        header="TỔNG TIỀN"
                        style="font-size: 14px"
                    ></Column>
                    <Column field="action" header="HÀNH ĐỘNG" style="font-size: 14px">
                        <template #body="slotProps">
                            <router-link to="/client/order-detail">
                                <div class="text-blue-400 cursor-pointer hover:underline">
                                    {{ slotProps.data.action
                                    }}<i class="fa-solid fa-arrow-right ml-2"></i>
                                </div>
                            </router-link>
                        </template>
                    </Column>
                </DataTable>
                <Paginator
                    :rows="10"
                    :totalRecords="120"
                    :rowsPerPageOptions="[10, 20, 30]"
                ></Paginator>
            </div>
        </div>
        <div
            v-if="0"
            class="border-1 border-round-md border-solid border-gray-200 mt-4 bg-white"
        >
            <div class="font-medium border-bottom-1 border-gray-300 bg-white">
                <div class="mx-3 mt-4 flex justify-content-between">
                    <p class="">SẢN PHẨM NỔI BẬt</p>
                    <a href="" class="text-orange-400 hover:underline"
                        >Xem tất cả <i class="fa-solid fa-arrow-right ml-2"></i
                    ></a>
                </div>
            </div>
            <div class="mt-5">
                <div class="flex flex-wrap justify-content-between">
                    <div
                        v-for="item in searchProducts"
                        :key="item.id"
                        class="lg:col-3 md:col-6 col-12 text-sm border-1 border-solid border-gray-200 border-round-sm p-3"
                    >
                        <img
                            class="w-full h-48 object-cover"
                            :src="
                                item.itM1.length
                                    ? item.itM1[0].filePath
                                    : 'https://placehold.co/200'
                            "
                            alt="Product Image"
                        />
                        <div class="mt-2">
                            <div class="flex items-center">
                                <Rating v-model="item.rate" :cancel="false" />
                                <label class="ml-2">({{ item.rate || 0 }})</label>
                            </div>
                            <div
                                class="my-2 text-base font-medium cursor-pointer hover:underline"
                            >
                                {{ item.itemName }}
                            </div>
                            <div class="mb-2 text-base text-blue-400">
                                {{ formatCurrency(item.price) }}
                            </div>
                        </div>
                    </div>
                </div>
                <Paginator :rows="10" :totalRecords="120"></Paginator>
            </div>
        </div>
    </div>
</template>

<script setup>
import API from "@/api/api-main";
import { format } from "date-fns";
import { ref, onMounted } from "vue";
import OrderStats from "../../home/components/OrderStats.vue";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const addressFields = ["address", "locationName", "areaName"];
const getAddress = (user) => {
    if (!user) {
        return "";
    }
    let arr = [];
    addressFields.forEach((key) => {
        if (user[key]) {
            arr.push(user[key]);
        }
    });
    return arr.join(", ");
};

const products = ref([
    {
        productCode: "#96459761",
        status: "ĐANG GIAO HÀNG",
        orderDate: "05/07/2024",
        total: "10.500.000đ (5 sản phẩm)",
        action: "Xem chi tiết",
    },
    {
        productCode: "#712123",
        status: "ĐÃ HOÀN TẤT",
        orderDate: "02/25/2024",
        total: "25.000.000đ (11 sản phẩm)",
        action: "Xem chi tiết",
    },
    {
        productCode: "#712625",
        status: "HỦY",
        orderDate: "20/04/2024",
        total: "1.500.000đ (3 sản phẩm)",
        action: "Xem chi tiết",
    },
    {
        productCode: "#712970",
        status: "ĐÃ HOÀN TẤT",
        orderDate: "24/03/2024",
        total: "500.000đ (1 sản phẩm)",
        action: "Xem chi tiết",
    },
    {
        productCode: "#712970",
        status: "HỦY",
        orderDate: "11/02/2024",
        total: "450.000đ (1 sản phẩm)",
        action: "Xem chi tiết",
    },
    {
        productCode: "#712970",
        status: "ĐÃ HOÀN TẤT",
        orderDate: "07/12/2024",
        total: "690.000đ (1 sản phẩm)",
        action: "Xem chi tiết",
    },
]);

const searchProducts = ref([]);

const getSeverity = (status) => {
    switch (status) {
        case "HỦY":
            return "danger";

        case "ĐÃ HOÀN TẤT":
            return "success";

        case "ĐANG GIAO HÀNG":
            return "warning";

        case "renewal":
            return null;
    }
};

const formatCurrency = (value) => {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(value);
};

const getRowClass = (rowData) => {
    if (rowData.status === "Đang giao hàng") {
        return "completed-row";
    } else {
        return "";
    }
};
const contact = ref(null);
const delivery = ref(null);
const dashboard = ref({});
const username = ref(null);
const user = ref({});

import { useMeStore } from "../../../../../Pinia/me";
const meStore = useMeStore();
const initialComponent = async () => {
    username.value = localStorage.getItem("username") || "";
    const meData = await meStore.getMe();
    user.value = meData?.user;
    if (meData) {
        contact.value = meData?.user.bpInfo.crD5.filter((row) => row.default == "Y")[0];
        delivery.value = meData?.user.bpInfo.crD1.filter(
            (row) => row.default == "Y" && row.type == "S"
        )[0];
    }
    API.get("dashboard")
        .then((res) => {
            dashboard.value = res.data;
        })
        .catch((error) => {});
};

const getAddressLabel = (obj) => {
    return [obj.address, obj.locationName, obj.areaName].join(", ");
};

onMounted(() => {
    initialComponent();
});
</script>

<style scoped>
.success {
    color: #2db224;
}

.warning {
    color: #fa8232;
}

.danger {
    color: #ee5858;
}
</style>
