<script lang="ts" async setup>
import { ref, onMounted } from "vue";
import { AxiosResponse } from "axios";
import API from "../api/api-main";

interface Claim {
    key?: string;
    id: number;
    code: string;
    privilegeName: string;
    numberOrder: number;
    parentId: any | null;
    children: Array<Claim>;
}

interface Prop {
    claims?: Array<Claim>;
    expandFirst?: boolean | undefined;
}

const modelValue = defineModel<Object>();
const expandedKeys = ref<Array<{ [key: string]: boolean }>>([]);
const props = defineProps<Prop>();
const claims = ref<Array<Claim>>([]);

const fetchClaims = (): void => {
    API.get("Privileges/getall")
        .then((res: AxiosResponse) => {
            const data = res.data as Array<Claim>;
            claims.value = mappingClaims(data);
            handleExpandFirstLine();
        })
        .catch();
};

const mappingClaims = (src: Array<Claim>): Array<Claim> => {
    const result = src;
    if (result.length) {
        result.forEach((item, i) => {
            item.key = item.id?.toString();
            if (item.children && item.children.length) {
                mappingClaims(item.children);
            }
        });
    }
    return result;
};

const handleExpandFirstLine = (): void => {
    if (props.expandFirst || props.expandFirst === undefined)
        claims.value
            ?.map((item) => item.key)
            ?.forEach((key) => {
                if (key !== undefined) {
                    let objKey = {};
                    objKey[key] = true;
                    expandedKeys.value.push(objKey);
                }
            });
};

onMounted((): void => {
    if (props.claims) {
        claims.value = mappingClaims(props.claims);
        handleExpandFirstLine();
    } else {
        fetchClaims();
    }
});
</script>
<template>
    <div class="grid m-0">
        <div v-for="(claim, i) in claims" :key="i" class="col-4">
            <Tree
                v-model:selectionKeys="modelValue"
                v-model:expandedKeys="expandedKeys[i]"
                :value="[claim]"
                selectionMode="checkbox"
            >
                <template #default="slotProps">
                    {{ slotProps.node.privilegeName }}
                </template>
            </Tree>
        </div>
    </div>
</template>

<style scoped>
.max-h-30rem {
    height: 30rem;
    max-height: 30rem;
    overflow-y: auto;
}
</style>
