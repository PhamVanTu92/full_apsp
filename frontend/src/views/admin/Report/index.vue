<template>
    <h4 class="font-bold mb-3">{{ t('body.report.report_list_title') }}</h4>
    <div class="card">
        <AutoComplete v-model="selected" :suggestions="suggestions" @complete="handlerSearch"
            @item-select="onSelectReport" optionLabel="name" :placeholder="t('body.report.search_placeholder_2')"
            class="mb-3" style="width: 452px" pt:input:class="w-full" :completeOnFocus="true">
            <template #option="sp">
                <div class="p-1 flex gap-3 align-items-center">
                    <i class="fa-solid fa-file-invoice text-2xl"></i>
                    <span class="">{{ sp.option.name }}</span>
                    <div class="p-1 text-sm border-1 border-round border-300 bg-gray-100 font-semibold">#{{
                        sp.option.category }}</div>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-center text-500">Không tìm thấy kết quả nào</div>
            </template>
        </AutoComplete>
        <TabView v-model:activeIndex="active" @tab-change="onChangeTabView" class="card p-0 overflow-hidden">
            <TabPanel :header="cateName" v-for="(cateName, i) in getCategories()" :key="i">
                <DataTable selectionMode="single" :value="reports.filter((item) => item.category == cateName)" :pt="{
                    headerrow: {
                        class: 'hidden'
                    }
                }" class="border-top-1 border-200" showGridlines @row-click="onSelectReport">
                    <Column field="name" header="Tên báo cáo">
                        <template #body="{ data }">
                            <div class="p-2 flex justify-content-between align-items-center">
                                <div class="flex">
                                    <i class="fa-solid fa-file-invoice text-2xl"></i>
                                    <span class="ml-4">
                                        {{ data.name }}
                                    </span>
                                </div>
                                <i class="pi pi-arrow-right"></i>
                            </div>
                        </template>
                    </Column>
                </DataTable>
            </TabPanel>
        </TabView>
    </div>
</template>
<script setup lang="ts">
import uniq from 'lodash/uniq';
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const router = useRouter();
const route = useRoute();

const selected = ref();
const suggestions = ref<any[]>([]);
const active = ref<number>(0);

const onChangeTabView = (event: any) => {

    router.replace({
        name: route.name,
        query: {
            tab: event.index
        }
    });
};

const onSelectReport = ({ value, data }: any) => {
    const path = value?.link || data?.link;
    router.push(path);
};

const compareFn = (row: any, key: any) => {
    let _key = removeAccents(key?.trim().toLowerCase());
    if (_key) {
        return removeAccents(row.name.toLowerCase()).includes(_key) || removeAccents(row.category.toLowerCase()).includes(_key);
    }
    return true;
};

const handlerSearch = ({ query }: any) => {
    suggestions.value = reports.filter((item) => compareFn(item, query));
};

const reports = [
    {
        name: t('body.report.product_sales_report'),
        link: '/report/buy-by-product',
        category: t('body.report.sales_tab')
    },
    {
        name: t('body.report.order_sales_report'),
        link: '/report/purchase-report-by-order',
        category: t('body.report.sales_tab')
    },
    {
        link: '/report/accounts-payable',
        name: t('body.report.report_title_customer_debt'),
        category: t('body.report.finance_tab')
    },
    {
        link: '/report/debt-ledger',
        name: t('body.report.report_title_debt_details_by_object'),
        category: t('body.report.finance_tab')
    },
    {
        link: '/report/payment',
        name: t('body.report.report_title_immediate_payment_bonus'),
        category: t('body.report.finance_tab')
    },
    {
        link: '/report/committed',
        name: t('body.report.report_title_volume_commitment'),
        category: t('body.report.finance_tab')
    },
    {
        link: '/report/inventory-report',
        name: t('body.report.report_title_inventory'),
        category: t('body.report.warehouse_tab')
    },
    {
        link: '/report/average-price',
        name: t('body.report.report_title_average_price'),
        category: t('body.report.sales_tab')
    },
    {
        link: '/report/return-goods',
        name: t('ReturnedGoods.reportTitle'),
        category: t('body.report.sales_tab')
    },
    {
        link: '/report/consent-log',
        name: t('body.report.consentlog_report_title'),
        category: t('body.report.system_log_tab')
    },
    {
        link: '/report/system-log',
        name: t('body.report.system_log_title'),
        category: t('body.report.system_log_tab')
    },
    {
        link: '/report/bonus-point',
        name: t('body.report.bonus_point'),
        category: t('body.report.bonus_point_tab')
    },
    {
        link: '/report/report-zalo',
        name: t('body.report.title_report_zalo'),
        category: "Zalo"
    },
];

const getCategories = () => {
    return uniq(reports.map((el) => el.category));
};

function removeAccents(str: string) {
    var AccentsMap = ['aàảãáạăằẳẵắặâầẩẫấậ', 'AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ', 'dđ', 'DĐ', 'eèẻẽéẹêềểễếệ', 'EÈẺẼÉẸÊỀỂỄẾỆ', 'iìỉĩíị', 'IÌỈĨÍỊ', 'oòỏõóọôồổỗốộơờởỡớợ', 'OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ', 'uùủũúụưừửữứự', 'UÙỦŨÚỤƯỪỬỮỨỰ', 'yỳỷỹýỳ', 'YỲỶỸÝỴ'];
    for (var i = 0; i < AccentsMap.length; i++) {
        var re = new RegExp('[' + AccentsMap[i].substr(1) + ']', 'g');
        var char = AccentsMap[i][0];
        str = str.replace(re, char);
    }
    return str;
}

onMounted(() => {
    if (route.query.tab) {
        active.value = Number(route.query.tab);
    }
});
</script>

<style>
.search-style {
    border: none;
    outline: none;
}
</style>
