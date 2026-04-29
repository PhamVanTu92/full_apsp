<template>
    <div class="flex flex-column gap-3">
        <div class="flex justify-content-between align-items-center">
            <strong class="text-2xl">{{t('client.shipping_list')}}</strong>
            <AddOrdersShipComp></AddOrdersShipComp>
        </div>
        <div class="card flex flex-column gap-3">
            <DataTable
                paginator
                :rowsPerPageOptions="[5, 10, 20, 50]"
                :totalRecords="pagination.total"
                :rows="pagination.limit"
                :page="pagination.page"
                @page="onPageChange($event)"
                lazy
                :value="ProductsInTable"
                showGridlines
                stripedRows
            >
                <template #empty>
                    <div class="text-center py-5 my-5">
                       {{t('client.no_matching_products')}}
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between flex-wrap gap-2">
                        <ProductFilter @change="onChangeFilter" :clearable="true">
                        </ProductFilter>
                    </div>
                </template>
                <Column :header="t('client.stt')">
                    <template #body="slotProps">
                        <div class="text-right">
                            {{ slotProps.index + 1 + pagination.page * pagination.limit }}
                        </div>
                    </template>
                </Column>
                <Column field="itemName" :header="t('client.product_name')"></Column>
                <Column field="ougp.ouom.uomName" :header="t('client.unit')"></Column>
                <Column field="onHand" :header="t('client.warehouse_stock')" style="width: 190px">
                    <template #body="slotProps">
                        <div class="text-right">
                            {{ slotProps.data.onHand }}
                        </div>
                    </template>
                </Column>
                <Column field="onOrder" :header="t('client.pending_approval')" style="width: 190px">
                    <template #body="slotProps">
                        <div class="text-right">
                            {{ slotProps.data.onOrder }}
                        </div>
                    </template>
                </Column>
                <Column :header="t('client.ordered_quantity')" style="width: 150px">
                    <template #body="slotProps">
                        <div class="text-right">
                            {{
                                (slotProps.data.onHand || 0) -
                                (slotProps.data.onOrder || 0)
                            }}
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>

    <loading v-if="isLoading"></loading>
</template>

<script setup lang="js">
import { computed, onMounted, ref } from "vue";
import API from "@/api/api-main";
import ProductFilter from "@/components/ProductFilter.vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const isLoading = ref(false)
const filterQuery = ref(null)
const ProductsInTable = ref([]);
const pagination = ref({
  page: 0,
  limit: 10,
  total: 0
})
const onChangeFilter = async (query) => {
  filterQuery.value = query
  await GetProducts(query);
};
const GetProducts = async (query = null) => {
  isLoading.value = true
  try {
    let url = `Item`;
    if (query) {
      url += query + `&Page=${pagination.value.page + 1}&PageSize=${pagination.value.limit}&typeDoc=YCLHG`;
    } else {
      url += `?Page=${pagination.value.page + 1}&PageSize=${pagination.value.limit}&typeDoc=YCLHG`;
    }
    const res = await API.get(url);
    ProductsInTable.value = res.data.items
    // pagination.value.page = res.data.skip
    // pagination.value.limit = res.data.limit
    pagination.value.total = res.data.total
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false
  }
};



const onPageChange = (e) => {
  pagination.value.page = e.page
  pagination.value.limit = e.rows
  GetProducts(filterQuery.value)
}

onMounted(() => {
  GetProducts()
})
</script>
