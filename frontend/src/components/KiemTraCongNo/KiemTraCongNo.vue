<template>
  <div class="flex">
    <Button
      v-if="!props.bpId"
      icon="fa-solid fa-dollar-sign"
      disabled
      severity="secondary"
      :label="t('body.OrderList.check_balance_button')"
      class="w-full"
    />
    <Button
      v-else
      icon="fa-solid fa-dollar-sign"
      :label="t('body.OrderList.check_balance_button')"
      :severity="buttonSeverity"
      :loading="loading"
      class="w-full"
      @click="visible = true"
    />
    <Dialog
      v-model:visible="visible"
      :header="
        t('body.report.report_title_customer_debt') +
        (props.bpName ? ` - ${props.bpName}` : '')
      "
      modal
      class="w-9"
      style="max-width: 72rem"
    >
      <div class="flex">
        <DataTable
          :value="[
            ...props.invoices,
            ...debtData?.map((el) => el.invoices)?.flatMap((val) => val),
          ]"
          showGridlines
          scrollable
          scrollHeight="30rem"
          class="border-1 border-200 flex-grow-1 overflow-auto"
          stripedRows
          resizableColumns
          columnResizeMode="fit"
          groupRowsBy="invoiceNumber"
          rowGroupMode="rowspan"
          :rowClass="rowClass"
        >
          <Column header="#">
            <template #body="sp">
              <span
                :class="
                  calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) !== '-'
                    ? 'text-red-500'
                    : ''
                "
              >
                {{ sp.index + 1 }}
              </span>
            </template>
          </Column>
          <Column
            field="invoiceNumber"
            :header="t('body.report.order_or_request_code_column')"
          >
            <template #body="sp">
              <Button
                icon="pi pi-arrow-right"
                text
                :label="sp.data.invoiceNumber"
                class="p-0 font-semibold"
                :class="
                  calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) !== '-'
                    ? 'text-red-500'
                    : ''
                "
              />
            </template>
          </Column>
          <Column
            field="paymentMethodName"
            :header="t('body.sampleRequest.customer.debt_form_column')"
          >
            <template #body="sp">
              <span
                :class="
                  calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) !== '-'
                    ? 'text-red-500'
                    : ''
                "
              >
                {{ sp.data.paymentMethodName }}
              </span>
            </template>
          </Column>
          <Column field="" :header="t('body.systemSetting.overdue_debt')">
            <template #body="sp">
              <span
                :class="
                  calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) !== '-'
                    ? 'text-red-500'
                    : ''
                "
              >
                {{ calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) }}
              </span>
            </template>
          </Column>
          <Column
            field="amountOverdue"
            :header="t('body.report.table_header_total_value')"
            class="text-right"
          >
            <template #body="sp">
              <span
                :class="
                  calculateOverdueDays(sp.data.dueDate, sp.data.amountOverdue) !== '-'
                    ? 'text-red-500'
                    : 'text-primary'
                "
              >
                {{ Intl.NumberFormat().format(sp.data.amountOverdue) }}
              </span>
            </template>
          </Column>
          <template #empty>
            <div class="py-6 my-6 text-center font-500 font-italic h-full">
              {{ t("body.systemSetting.no_data_to_display") }}
            </div>
          </template>
        </DataTable>
        <div class="flex flex-column flex-grow-1 border-2 border-200">
          <div
            style="padding: 10.5px"
            class="border-bottom-1 border-200 font-semibold text-centerr w-20rem"
          >
            {{ t("body.report.table_header_total_value") }}
          </div>
          <div class="flex flex-column justify-content-center flex-grow-1">
            <div class="my-auto p-3 flex flex-column gap-3">
              <div v-for="row in debtData" :key="row.id" class="font-semibold">
                <div class="text-lg font-semibold mb-2">{{ row.paymentMethodName }}:</div>
                <!-- v-tooltip.top="{
                                        value: `Dư nợ: ${Intl.NumberFormat().format(
                                            row.balance
                                        )} \n Phát sinh: ${Intl.NumberFormat().format(
                                            getPayType(row.paymentMethodCode)
                                        )}`,
                                        pt: {
                                            text: 'surface-700 font-medium',
                                            root: 'block',
                                        },
                                    }" -->
                <div
                  class="text-left p-2 border-1 border-300"
                  :class="{
                    'text-red-500':
                      isNumber(row.balance) &&
                      getPayType(row.paymentMethodCode) > row.balance,
                    'text-primary':
                      row.balance == null ||
                      getPayType(row.paymentMethodCode) <= row.balance,
                  }"
                >
                  <div class="flex justify-content-between align-items-center">
                    <div>
                      {{ fnum(getPayType(row.paymentMethodCode)) }} /
                      {{ fnum(row.balance) }}
                    </div>
                    <Button
                      icon="pi pi-info-circle"
                      class="p-0"
                      text
                      style="width: 20px; height: 20px"
                      @click="
                        onClickShowInfo(
                          $event,
                          row.balance,
                          getPayType(row.paymentMethodCode),
                          row.balanceLimit
                        )
                      "
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- {{ props.invoices }} -->
      <template #footer>
        <Button
          :label="t('body.OrderList.close')"
          class="ml-auto"
          @click="visible = false"
          severity="secondary"
        />
      </template>
    </Dialog>
    <ConfirmPopup group="headless">
      <!-- , acceptCallback, rejectCallback -->
      <template #container="{ message }">
        <div class="border-round p-3">
          <div v-if="message.message.addValue">
            {{ t("body.sampleRequest.customer.incurred") }}
            <span class="font-semibold">
              {{ Intl.NumberFormat().format(message.message.addValue) }} VND
            </span>
          </div>
          <div>
            {{ t("body.sampleRequest.customer.current_debt") }}:
            <span class="font-semibold">
              {{ Intl.NumberFormat().format(message.message.value) }} VND
            </span>
          </div>
          <div>
            {{ t("body.sampleRequest.customer.credit_limit") }}:
            <span class="font-semibold">
              {{ Intl.NumberFormat().format(message.message.limit) }} VND
            </span>
          </div>
        </div>
      </template>
    </ConfirmPopup>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useConfirm } from "primevue/useconfirm";
import API from "../../api/api-main";
import { isNumber } from "lodash";
const { t } = useI18n();

interface Props {
  bpId: number | any;
  bpName?: string | any;
  payCredit?: number | undefined;
  payGuarantee?: number | undefined;
  invoices?: Array<Invoice>;
  statusOrder: string | any;
}
type PaymentMethodCode = "PayGuarantee" | "PayCredit" | string;
interface Invoice {
  id: number;
  invoiceNumber: string;
  invoiceDate: string | Date;
  dueDate: string | Date;
  paymentMethodID: number;
  paymentMethodCode: PaymentMethodCode;
  paymentMethodName: string;
  invoiceTotal: number;
  paidAmount: any;
  amountOverdue: number;
  typeOfDebt: string;
  bpId: number;
  status: string;
  payments: any;
  isInvoice?: boolean;
}
interface Debt {
  id: number;
  paymentMethodID: number;
  paymentMethodCode: PaymentMethodCode;
  paymentMethodName: string;
  balanceLimit: number;
  balance: number;
  bankGuarantee: string;
  startLetterOfGuarantee: string;
  letterOfGuarantee: string;
  invoices: Array<Invoice>;
  bpId: number;
  status: null;
}

const rowClass = (data: Invoice) => {
  return [{ "bg-green-100c    ": data.isInvoice === false }];
};

const isExceedDebt = defineModel("isExceedDebt", {
  default: false,
  type: Boolean,
});
const visible = ref(false);
const props = withDefaults(defineProps<Props>(), {
  invoices: () => [],
});
const loading = ref(false);
const taxOver = ref<Array<string>>([]);
const debtData = ref<Array<Debt>>([]);
const confirm = useConfirm();

const onClickShowInfo = (event: any, value: number, addValue: number, limit: number) => {
  confirm.require({
    target: event.currentTarget,
    group: "headless",
    message: {
      value: limit,
      addValue: addValue,
      limit: value,
    },
    accept: () => {},
    reject: () => {},
  });
};
const getPayType = (code: string): number => {
  try {
    const debtItem = debtData.value.find((item) => item.paymentMethodCode === code);
    if (code == "PayCredit") {
      // if (props.statusOrder && props.statusOrder !== 'CXN')
      return debtItem?.balanceLimit ?? 0;
      // else
      //     return (props.payCredit ?? 0) + (debtItem?.balanceLimit ?? 0);
    } else if (code == "PayGuarantee") {
      // if (props.statusOrder && props.statusOrder !== 'CXN')
      return debtItem?.balanceLimit ?? 0;
      // else
      //     return (props.payGuarantee ?? 0) + (debtItem?.balanceLimit ?? 0);
    }
    return 0;
  } catch (error) {
    console.error("getPayType" + error);
    return 0;
  }
};

const calculateOverdueDays = (dueDate: string | Date, amountOverdue: number) => {
  // nếu không có ngày đến hạn hoặc không có số tiền quá hạn thì trả về '-'
  if (!dueDate || amountOverdue > 0) return "-";
  const today = new Date();
  const due = new Date(dueDate);
  if (due < today) {
    const timeDiff = today.getTime() - due.getTime();
    const daysDiff = Math.floor(timeDiff / (1000 * 3600 * 24));
    return daysDiff + 1;
  } else {
    return "-";
  }
};
const emit = defineEmits<{
  (e: "update:isExceedDebt", value: boolean): void;
}>();
const buttonSeverity = computed(() => {
  let limitZeroCount = 0;
  taxOver.value = [];
  if (debtData.value.length < 1) return "secondary";
  for (const row of debtData.value) {
    //  row.balance -> Hạn mức
    //  row.balanceLimit -> dư nợ
    switch (true) {
      case row.balance != null && row.balance < getPayType(row.paymentMethodCode):
        taxOver.value.push(row.paymentMethodCode);
        break;
      case checkPayOverdueDebt(row):
        taxOver.value.push(row.paymentMethodCode);
        break;
      case row.balance == null:
        limitZeroCount++;
        break;
    }
  }
  switch (true) {
    case taxOver.value.length > 0:
      isExceedDebt.value = true;
      emit("update:isExceedDebt", true);
      return "danger";
    case limitZeroCount == 2:
      isExceedDebt.value = false;
      emit("update:isExceedDebt", false);
      return "secondary";
    default:
      isExceedDebt.value = false;
      emit("update:isExceedDebt", false);
      return "primary";
  }
});

const fetchDetbData = (id: number) => {
  debtData.value = [];
  taxOver.value = [];
  if (id) {
    loading.value = true;
    API.get(`customer/${id}/debt`)
      .then((response) => {
        debtData.value = response.data;
      })
      .catch((error) => {
        console.error(error);
      })
      .finally(() => {
        loading.value = false;
      });
  }
};

const fnum = (value: any) => {
  if (isNumber(value)) {
    return Intl.NumberFormat().format(value);
  } else {
    if (value == null) return t("body.sampleRequest.customer.no_credit_limit");
    else `${value}`;
  }
};

watch(
  () => props.bpId,
  (id) => {
    fetchDetbData(id);
  }
);

onMounted(function () {
  if (props.bpId) {
    fetchDetbData(props.bpId);
  }
});

const isDebtOver = computed(() => taxOver.value.length > 0);

const checkPayOverdueDebt = (val: any) => {
  if (isNumber(val.balance) && val.balanceLimit > val.balance) return true;
  for (let invoice of val.invoices)
    if (calculateOverdueDays(invoice.dueDate, invoice.amountOverdue) !== "-") return true;
  return false;
};
defineExpose({
  isDebtOver,
});
</script>

<style scoped></style>
