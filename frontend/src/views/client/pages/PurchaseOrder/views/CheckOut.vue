<template>
  <div class="bg-gray-300 p-4">
    <div class="flex container mx-6 mx-auto">
      <div><i class="pi pi-home mr-2"></i><span>Trang chủ</span></div>
      <div>
        <!-- <i class="pi pi-arrow-right mr-2 ml-2"></i
        > -->
        <span class="text-primary">Thanh toán</span>
      </div>
    </div>
  </div>
  <div class="container mx-7 mt-5 mx-auto">
    <div class="grid">
      <div class="col-8 flex flex-column justify-content-between">
        <div class="">
          <h6 class="text-lg font-semibold">Thông tin khách hàng</h6>
          <div class="p-fluid">
            <div class="grid align-items-end">
              <div class="col-6">
                <div class="field">
                  <label>Công ty(Optional)</label>
                  <InputText :value="modelState.partner.company" readonly />
                </div>
              </div>
              <div class="col-3">
                <div class="field">
                  <label>Họ và tên</label>
                  <InputText :value="modelState.partner.last_name" readonly />
                </div>
              </div>
              <div class="col-3">
                <div class="field">
                  <InputText :value="modelState.partner.first_name" readonly />
                </div>
              </div>
            </div>
            <div class="grid align-items-end">
              <div class="col-6">
                <div class="field">
                  <label>Email</label>
                  <InputText :value="modelState.partner.email" readonly />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label>Số điện thoại</label>
                  <InputText :value="modelState.partner.phone_number" readonly />
                </div>
              </div>
            </div>
            <div class="grid align-items-end">
              <div class="col-12">
                <div class="field">
                  <label>Địa chỉ nhận hàng</label>
                  <InputGroup>
                    <InputText
                      :value="toStringAddress(modelState.deliveryAddress)"
                      readonly
                    />
                    <Button
                      label="Thay đổi"
                      outlined
                      class="border-300 text-500"
                      @click="changeDeliveryAddress"
                    />
                  </InputGroup>
                </div>
              </div>
            </div>
            <div class="grid align-items-end">
              <div class="col-12">
                <div class="field">
                  <label>Địa chỉ xuất hoá đơn</label>
                  <InputGroup>
                    <InputText
                      :value="toStringAddress(modelState.invoicingAddress)"
                      readonly
                    />
                    <Button
                      label="Thay đổi"
                      outlined
                      class="border-300 text-500"
                      @click="changeInvoicingAddress"
                    />
                  </InputGroup>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="w-full py-6">
          <Button label="Quay lại" @click="router.back()"/>
        </div>
        <!-- <div class="border-1 border-300 border-round mt-4">
          <div class="p-3 pb-0 text-lg font-semibold">Phương thức thanh toán
          </div>
          <hr />
          <div class="flex">
            <div style="width: 20%" class="p-3">
              <div class="flex flex-column align-items-center gap-3 border-right-1 border-bluegray-200">
                <img src="/image/cash-on-delivery.png" alt="" style="width: 32px; height: 32px" />
                <span>COD</span>
                <RadioButton inputId="ingredient1" name="pizza" v-model="methodPay" value="1" />
              </div>
            </div>
            <div style="width: 20%" class="p-3">
              <div class="flex flex-column align-items-center gap-3 border-right-1 border-bluegray-200">
                <img src="/image/MoMo_Logo.png" alt="" style="width: 32px; height: 32px" />
                <span>Ví Momo</span>
                <RadioButton inputId="ingredient1" name="pizza" v-model="methodPay" value="3" />
              </div>
            </div>

            <div style="width: 20%" class="p-3">
              <div class="flex flex-column align-items-center gap-3 border-right-1 border-bluegray-200">
                <img src="/image/bank.png" alt="" style="width: 32px; height: 32px" />
                <span>Chuyển khoản NH</span>
                <RadioButton inputId="ingredient1" name="pizza" v-model="methodPay" value="4" />
              </div>
            </div>
            <div style="width: 20%" class="p-3">
              <div class="flex flex-column align-items-center gap-3">
                <img src="/image/atm-card.png" alt="" style="width: 32px; height: 32px" />
                <span>Debit/Credit Card</span>
                <RadioButton inputId="ingredient1" name="pizza" v-model="methodPay" value="5" />
              </div>
            </div>
            <div style="width: 20%" class="p-3">
              <div class="flex flex-column align-items-center gap-3 border-left-1 border-bluegray-200">
                <img src="/image/calendar.png" alt="" style="width: 32px; height: 32px" />
                <span>Phương thức khác</span>
                <RadioButton inputId="ingredient1" name="pizza" v-model="methodPay" value="6" />
              </div>
            </div>
          </div>
          <hr />
          <div class="p-5 p-fluid">
            <div class="grid align-items-end">
              <div class="col-12">
                <div class="field">
                  <label>Tên trên thẻ</label>
                  <InputText />
                </div>
              </div>
              <div class="col-12">
                <div class="field">
                  <label>Số thẻ</label>
                  <InputText />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label>Ngày hết hạn</label>
                  <InputNumber />
                </div>
              </div>
              <div class="col-6">
                <div class="field">
                  <label>CVC</label>
                  <InputNumber />
                </div>
              </div>
            </div>
          </div>
        </div> -->
        <!-- <div class="mt-4">
          <div>
            <h6 class="text-lg font-semibold">Thông tin khác</h6>
            <span class="">Ghi chú (Optional)</span>
            <div class="p-1 mt-2">
              <Textarea v-model="modelState.note" class="w-full" rows="5" placeholder="Thêm ghi chú" />
            </div>
          </div>
        </div> -->
      </div>
      <div class="col-4">
        <div class="card">
          <span class="text-lg font-semibold">Tóm tắt đơn hàng</span>
          <DataTable
            :value="store.orders"
            :pt="{ thead: { style: 'display:none' }, footer: { style: 'padding:0px' } }"
          >
            <Column style="width: 5rem">
              <template #body="sp">
                <Image :src="sp.data.thumbnail_url" width="60px" height="60px" preview />
              </template>
            </Column>
            <Column field="name">
              <template #body="sp">
                <div>
                  <div class="mb-1">{{ sp.data.name }}</div>
                  <div class="flex justify-content-between">
                    <span class="text-green-700">{{
                      format.FormatCurrency(sp.data.u_price)
                    }}</span>
                    <span class="ml-3 font-semibold"
                      >(x{{ sp.data.quantity }}) ({{
                        sp.data.unit_group.unit_conversions.find(
                          (el) => el.to_unit_id == sp.data.unit_group.base_unit_id
                        ).to_unit_name
                      }})</span
                    >
                  </div>
                </div>
              </template>
            </Column>
            <template v-if="store.gifts.length > 0" #footer>
              <DataTable :value="store.gifts" :pt="{ thead: { style: 'display:none' } }">
                <Column>
                  <template #body="sp">
                    <Image
                      :src="sp.data.thumbnail_url"
                      width="60px"
                      height="60px"
                      preview
                    />
                  </template>
                </Column>
                <Column field="name">
                  <template #body="sp">
                    <div>
                      <div class="mb-1">
                        <span
                          style="padding: 0.1rem"
                          class="text-sm border-1 border-green-600 text-green-600 mr-2"
                          >Quà tặng</span
                        >{{ sp.data.name }}
                      </div>
                      <div class="flex justify-content-between">
                        <span class="text-green-700">
                          <span class="line-through text-400 mr-2">
                            {{ format.FormatCurrency(sp.data.price) }}
                          </span>
                          <span>0 đ</span>
                        </span>
                        <span class="ml-3 font-semibold">(x{{ sp.data.quantity }})</span>
                      </div>
                    </div>
                  </template>
                </Column>
              </DataTable>
            </template>
          </DataTable>
          <div class="p-2 mt-3">
            <div class="flex justify-content-between">
              <h6>Tổng tiền</h6>
              <span class="text-green-700">{{
                format.FormatCurrency(store.totalItemPrice)
              }}</span>
            </div>
            <div class="flex justify-content-between">
              <h6>Phí vận chuyển</h6>
              <span class="text-green-700">{{ format.FormatCurrency(0) }}</span>
            </div>
            <div class="flex justify-content-between">
              <h6>Giảm giá</h6>
              <span class="text-red-600"
                >-{{ format.FormatCurrency(store.discount) }}</span
              >
            </div>
            <div class="flex justify-content-between">
              <h6>Thuế VAT</h6>
              <span class="text-green-700">0 đ</span>
            </div>
            <div class="flex justify-content-between align-items-center">
              <h6 class="m-0">Đặt cọc</h6>
              <div class="flex gap-2">
                <InputNumber
                  @blur="test('%')"
                  class="input-per"
                  v-model="modelState.hd_per"
                  suffix=" %"
                  :min="0"
                  :max="100"
                />
                <InputNumber
                  @blur="test('VND')"
                  class="input-money"
                  suffix=" đ"
                  v-model="modelState.hd_amount"
                />
              </div>
            </div>
            <hr />
          </div>
          <div class="p-2">
            <div class="flex justify-content-between">
              <h5>Thành tiền</h5>

              <span class="text-green-700">{{
                format.FormatCurrency(store.totalItemPrice - store.discount)
              }}</span>
            </div>
            <!-- <router-link to="/client/success">
            </router-link> -->
            <Button
              label="ĐẶT HÀNG"
              icon="pi pi-arrow-right"
              severity="warning"
              class="w-full p-3 mt-2 font-bold"
              iconPos="right"
              raised
              @click="onClickOrder"
              :loading="loadingOrder"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <Dialog
    v-model:visible="visibleDeliveryAddressChange"
    modal
    :draggable="false"
    header="Địa Chỉ Giao Hàng"
    :style="{ width: '40%' }"
    :closable="false"
  >
    <DataTable
      selectionMode="single"
      :pt="{ headerrow: { style: 'display:none' } }"
      v-model:selection="modelState.selectionDeliveryAddress"
      :value="address"
    >
      <Column selectionMode="single" headerStyle="width: 3rem"></Column>
      <Column header="Địa chỉ">
        <template #body="sp">
          <div class="line-height-3">
            <span>{{ toStringAddress(sp.data) }}</span>
            <span
              v-if="sp.data.is_default_shipping_address"
              class="border-1 border-green-600 text-green-600 m-2"
              >Mặc định</span
            >
          </div>
        </template>
      </Column>
      <template #footer>
        <Button label="Thêm địa chỉ mới" outlined icon="fa-solid fa-plus"/>
      </template>
    </DataTable>
    <template #footer>
      <Button
        label="Huỷ"
        @click="
          () => {
            visibleDeliveryAddressChange = false;
            modelState.selectionDeliveryAddress = modelState.deliveryAddress;
          }
        "
        outlined
      />
      <Button
        label="Xác nhận"
        @click="
          () => {
            modelState.deliveryAddress = modelState.selectionDeliveryAddress;
            visibleDeliveryAddressChange = false;
          }
        "
      />
    </template>
  </Dialog>
  <Dialog
    v-model:visible="visibleInvoiceAddressChange"
    modal
    :draggable="false"
    header="Địa Chỉ Xuất Hoá Đơn"
    :style="{ width: '40%' }"
    :closable="false"
  >
    <DataTable
      selectionMode="single"
      :pt="{ headerrow: { style: 'display:none' } }"
      v-model:selection="modelState.selectionInvoiceAddress"
      :value="address"
    >
      <Column selectionMode="single" headerStyle="width: 3rem"></Column>
      <Column header="Địa chỉ">
        <template #body="sp">
          <div class="line-height-3">
            <span>{{ toStringAddress(sp.data) }}</span>
            <span
              v-if="sp.data.is_billing_address"
              class="border-1 border-green-600 text-green-600 m-2"
              >Mặc định</span
            >
          </div>
        </template>
      </Column>
      <template #footer>
        <Button label="Thêm địa chỉ mới" outlined icon="fa-solid fa-plus"/>
      </template>
    </DataTable>
    <template #footer>
      <Button
        label="Huỷ"
        @click="
          () => {
            visibleInvoiceAddressChange = false;
            modelState.selectionInvoiceAddress = modelState.invoicingAddress;
          }
        "
        outlined
      />
      <Button
        label="Xác nhận"
        @click="
          () => {
            modelState.invoicingAddress = modelState.selectionInvoiceAddress;
            visibleInvoiceAddressChange = false;
          }
        "
      />
    </template>
  </Dialog>
</template>
<script setup>
import { onBeforeMount, reactive, ref, watchEffect } from "vue";
import { useOrderStore } from "@/Pinia/orderStore";
import { useRouter } from "vue-router";
import format from "../../../../../helpers/format.helper";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import axios from "axios";
import { authHeader_new } from "../../../../../helpers/auth-header.helper";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const dataform = new FormData();
const { toast, FunctionGlobal } = useGlobal();

const router = useRouter();

const modelState = reactive({
  addresses: null,
  selectionDeliveryAddress: {},
  selectionInvoiceAddress: {},
  partner: {},
  deliveryAddress: null,
  invoicingAddress: null,
  note: null,
  hd_per: 0,
  hd_amount: 0,
  deposit_type: "",
});

const loadingOrder = ref(false);
const onClickOrder = async () => {
  loadingOrder.value = true;
  const payload = {
    buyer_note: JSON.stringify({
      note: modelState.note,
      gift: store.gifts,
    }),
    contact_person_id: address.value ? address.value[0].id : 1,
    order_items: store.orders.map((el) => ({
      product_id: el.id,
      quantity: el.quantity,
      unit_id: el.unit_group.base_unit_id,
    })),
    dist_billing_address_id: modelState.invoicingAddress.id,
    dist_shipping_address_id: modelState.deliveryAddress.id,
    is_deposit: parseInt(modelState.hd_amount) > 0 ? true : false,
    total_deposit: modelState.hd_amount ? parseInt(modelState.hd_amount) : 0,
  };
  try {
    const res = await API.add("orders", payload);
    if (res) {
      if (parseInt(modelState.hd_per) > 0 || parseInt(modelState.hd_amount) > 0) {
        try {
          dataform.append("amount", parseInt(modelState.hd_amount));
          dataform.append("language", "vn");
          dataform.append("bankCode", "");
          dataform.append("REMOTE_ADDR", "192.168.1.1");
          dataform.append("vnp_TxnRef", res.data.new_order_id);
          dataform.append("token", authHeader_new().Authorization);
          const ress = await axios.post(
            "https://fox.ai.vn/vnpay_php/vnpay_create_payment.php",
            dataform
          );
          if (ress.data) {
            window.location.href = ress.data.url;
          }
        } catch (error) {
        } finally {
          loadingOrder.value = false;
        }
        return;
      } else {
        router.push({
          name: "order-success",
        });
      }
    }
  } catch (err) {
    toast.add({
      severity: "error",
      summary: "Thêm đơn hàng thất bại",
      detail: "Vui lòng thử lại sau",
      life: 3000,
    });
  }
  loadingOrder.value = false;
};

const visibleDeliveryAddressChange = ref(false);
const changeDeliveryAddress = () => {
  visibleDeliveryAddressChange.value = true;
};

const visibleInvoiceAddressChange = ref(false);
const changeInvoicingAddress = () => {
  visibleInvoiceAddressChange.value = true;
};

const address = ref([]);

const initialComponent = async () => {
  if (store.orders.length < 1) {
    router.push({ name: "order-home" });
  }
  // const resAddress = await API.get('/me/addresses');
  const resMe = await API.get("/me");
  address.value = resMe.data.addresses;

  // modelState.partner.company = resMe.data.name;
  modelState.partner.company = resMe.data.name;
  modelState.partner.email = resMe.data.email;
  modelState.partner.phone_number = resMe.data.phone_number;

  modelState.deliveryAddress = address.value.find((el) => el.is_default_shipping_address);
  modelState.selectionDeliveryAddress = modelState.deliveryAddress;
  modelState.invoicingAddress = address.value.find((el) => el.is_billing_address);
  modelState.selectionInvoiceAddress = modelState.invoicingAddress;
};

const adrProperties = [
  "house_number",
  "address_line1",
  "street",
  "ward",
  "district",
  "city",
];
const toStringAddress = (obj) => {
  if (!obj) return "";
  let result = [];
  for (const key of adrProperties) {
    if (obj[key]) result.push(obj[key]);
  }
  return result.join(", ");
};

onBeforeMount(() => {
  initialComponent();
});

const store = useOrderStore();

const test = (type) => {
  modelState.deposit_type = type;
};
watchEffect(() => {
  if (modelState.deposit_type === "%")
    modelState.hd_amount = ((store.totalItemPrice * modelState.hd_per) / 100).toFixed(2);
  if (modelState.deposit_type === "VND")
    modelState.hd_per = ((modelState.hd_amount / store.totalItemPrice) * 100).toFixed(2);
});

const getToken = async () => {
  const ress = await axios.get("https://fox.ai.vn/vnpay_php/vnpay_return.php");
  const sessionId = ress.data.session_id;
  document.cookie = `PHPSESSID=${sessionId}; path=/;`;



};
</script>

<style>
.input-per input {
  width: 5rem;
}

.input-money input {
  width: 10rem;
}
</style>
