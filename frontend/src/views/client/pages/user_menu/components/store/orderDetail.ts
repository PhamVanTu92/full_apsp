import { defineStore } from "pinia";
import { ref } from "vue";
import { Order } from "../types/orderDetail";
import API from "@/api/api-main";
import { AxiosResponse } from "axios"; 

function storeInit () {
    const isExceedDebt = ref<boolean>();
    const order = ref<Order | null>();
    const customer = ref<any>();
    const error = ref(false);

    function $reset () {
        order.value = null;
        customer.value = null;
        error.value = false;
        isExceedDebt.value = false;
    }

    function fetchStore(id? :number) {
        
        let _id: number = 0;
        if(id) {
            _id = id;
        }
        else if (order.value?.id) _id = order.value?.id
        else{
            error.value = true;
            return;
        }
        $reset();
        API.get(`PurchaseOrder/${_id}`)
        .then((res: AxiosResponse<{ item: Order }>) => {
            order.value = res.data["item"];
            if (order.value?.cardId) {
                API.get(`customer/${order.value.cardId}`).then((res) => {
                    customer.value = res.data;
                });
            }
        })
        .catch((error) => {
            error.value = true;
        });
    }

    return {
        order,
        customer,
        isExceedDebt,
        error,
        fetchStore,
        $reset
    }
}

export const useOrderDetailStore = defineStore('OrderDetail', storeInit)
