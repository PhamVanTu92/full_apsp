<template>
    <h4 class="font-bold mb-3">Báo cáo</h4>
    <div class="card">
        <AutoComplete
            v-model="selected"
            :suggestions="suggestions"
            @complete="handlerSearch"
            @item-select="onSelectReport"
            class="mb-3"
            optionLabel="name"
            placeholder="Tìm kiếm..."
            :completeOnFocus="true"
        >
            <template #option="sp">
                <div class="p-1 flex gap-3 align-items-center">
                    <i class="fa-solid fa-file-invoice text-2xl"></i>
                    <span class="">{{ sp.option.name }}</span>
                    <div class="p-1 text-sm border-1 border-round border-300 bg-gray-100">
                        #{{ sp.option.category }}
                    </div>
                </div>
            </template>
            <template #empty>
                <div class="p-5 my-5 text-center">
                    Không tìm thấy báo cáo nào phù hợp.
                </div>
            </template>
        </AutoComplete>
        <TabView class="card p-0 overflow-hidden">
            <TabPanel :header="cate" v-for="(cate, i) in getCategories()" :key="i">
                <DataTable
                    selectionMode="single"
                    :value="reports.filter((item) => item.category == cate)"
                    :pt="{
                        headerrow: {
                            class: 'hidden',
                        },
                    }"
                    class="border-top-1 border-200"
                    showGridlines=""
                    @row-click="onSelectReport"
                >
                    <Column field="name" header="Tên báo cáo">
                        <template #body="{ data }">
                            <div
                                class="p-2 flex justify-content-between align-items-center"
                            >
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
<script setup>
import { uniq } from "lodash";
import { ref } from "vue";
import { useRouter } from "vue-router";

const router = useRouter();

const selected = ref();
const suggestions = ref([]);

const onSelectReport = ({ value, data }) => {
    const path = value?.link || data?.link;
    router.push(path);
};

const compareFn = (row, key) => {
    let _key = removeAccents(key?.trim().toLowerCase());
    if (_key) {
        return (
            removeAccents(row.name.toLowerCase()).includes(_key) ||
            removeAccents(row.category.toLowerCase()).includes(_key)
        );
    }
    return true;
};

const handlerSearch = ({ query }) => {
    suggestions.value = reports.filter((item) => compareFn(item, query));
};

const reports = [
    {
        name: "Báo cáo mua hàng theo sản phẩm",
        link: "report/buy-by-product",
        category: "Mua hàng",
    },
    {
        name: "Báo cáo mua hàng theo đơn hàng",
        link: "report/purchase-by-order",
        category: "Mua hàng",
    },
    {
        name: "Báo cáo công nợ phải trả",
        link: "report/debt-payment",
        category: "Tài chính",
    },
    {
        name: "Báo cáo thống kê hóa đơn",
        link: "report/bill-summary",
        category: "Tài chính",
    },
    // {
    //     name: "Danh sách biên bản đối chiếu công nợ",
    //     link: "report/debt-reconciliation-data",
    //     category: "Tài chính",
    // },
    {
        name: "Báo cáo tồn kho hàng gửi",
        link: "report/inventory",
        category: "Kho",
    },
    // {
    //     name: "Danh sách biên bản xác nhận số lượng hàng gửi",
    //     link: "report/quantity-consigned-goods",
    //     category: "Kho",
    // },
    // {
    //     name: "Báo cáo kế hoạch nhập hàng",
    //     link: "report/purchase-plan",
    //     category: "Mua hàng",
    // },
    {
        name: "Báo cáo cam kết sản lượng",
        link: "report/commited",
        category: "Mua hàng",
    },
    // {
    //     name: "Báo cáo nhập, xuất, tồn hàng gửi",
    //     link: "report/inventory-send",
    //     category: "Mua hàng",
    // },
    // {
    //     name: "Báo cáo hàng tồn kho hàng gửi",
    //     link: "report/inventory",
    //     category: "Mua hàng",
    // },
    // {
    //     name: "Báo cáo cam kết sản lượng ",
    //     link: "report/commited",
    //     category: "Mua hàng",
    // },
    {
        name: "Báo cáo điểm thưởng khách hàng",
        link: "report/pointPromotion",
        category: "Điểm thưởng",
    },
];

const getCategories = () => {
    return uniq(reports.map((el) => el.category));
};

function removeAccents(str) {
    var AccentsMap = [
        "aàảãáạăằẳẵắặâầẩẫấậ",
        "AÀẢÃÁẠĂẰẲẴẮẶÂẦẨẪẤẬ",
        "dđ",
        "DĐ",
        "eèẻẽéẹêềểễếệ",
        "EÈẺẼÉẸÊỀỂỄẾỆ",
        "iìỉĩíị",
        "IÌỈĨÍỊ",
        "oòỏõóọôồổỗốộơờởỡớợ",
        "OÒỎÕÓỌÔỒỔỖỐỘƠỜỞỠỚỢ",
        "uùủũúụưừửữứự",
        "UÙỦŨÚỤƯỪỬỮỨỰ",
        "yỳỷỹýỵ",
        "YỲỶỸÝỴ",
    ];
    for (var i = 0; i < AccentsMap.length; i++) {
        var re = new RegExp("[" + AccentsMap[i].substr(1) + "]", "g");
        var char = AccentsMap[i][0];
        str = str.replace(re, char);
    }
    return str;
}
</script>

<style scoped></style>
