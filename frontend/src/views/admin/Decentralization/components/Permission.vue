<template>
    <div>
        <div v-if="0" class="card p-3 mb-3 flex gap-3 align-items-center">
            <span>Vai trò:</span>
            <Skeleton width="15rem" height="1.5rem" v-if="props.loading"></Skeleton>
            <span v-else class="font-semibold text-primary ml-2">{{
                props.user?.role?.name
            }}</span>
        </div>
        <div v-if="props.loading" class="card p-4 mb-3 flex gap-3">
            <div v-for="i in [1, 2, 3, 4]" :key="i" class="flex-grow-1">
                <Skeleton class="mb-3" height="1.2rem" />
                <div class="flex flex-column gap-3 pl-6">
                    <Skeleton class="" height="1.2rem" />
                    <Skeleton class="" height="1.2rem" />
                    <Skeleton class="" height="1.2rem" />
                    <Skeleton class="" height="1.2rem" />
                    <Skeleton class="" height="1.2rem" />
                </div>
            </div>
        </div>
        <ClaimsComponent
            v-else
            v-model="selectionClaim"
            class="card p-2 mb-3"
            expandFirst
        ></ClaimsComponent>
        <div class="flex gap-2 justify-content-end">
            <Button
                :disabled="props.loading"
                :loading="loading"
                @click="onClickSave"
                icon="pi pi-save"
                label="Lưu"
            />
        </div>
        <!-- {{ selectionClaim }} -->
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";

const toast = useToast();

const selectionClaim = ref({});
interface User {
    id: number;
    role: {
        name: string | null;
        roleClaims: Array<any>;
    };
    personRole: {
        name: string | null;
        roleClaims: Array<any>;
    };
}

interface Props {
    user: User | null;
    loading: boolean;
}
const props = defineProps<Props>();
const loading = ref(false);
const onClickSave = () => {
    loading.value = true;
    if (!props.user) {
        return;
    }
    const data = toPayload(selectionClaim.value);
    API.add(`Account/UserClaim/addupdate?UserId=${props.user?.id}`, data)
        .then((response) => {
            loading.value = false;

            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Cập nhật thành công",
                life: 3000,
            });
        })
        .catch((error) => {
            loading.value = false;
            console.error("Save failed", error);
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                life: 3000,
            });
        });

};

const toPayload = (obj: any): Array<any> => {
    const result = [] as Array<any>;
    if (obj)
        Object.keys(obj).forEach((key) => {
            const claim = {
                privilegesId: parseInt(key),
                checked: obj[key].checked,
                partialChecked: obj[key].partialChecked,
            };
            result.push(claim);
        });
    return result;
};

const convertClaimToSelect = (claims: Array<any>): Object => {
    const result = {};
    if (claims?.length > 0)
        claims.forEach((item) => {
            let { checked, partialChecked } = item;
            result[`${item.privilegesId}`] = {
                checked,
                partialChecked,
            };
        });
    return result;
};

watch(
    () => props.user,
    () => {
        if (props.user?.personRole && props.user?.personRole.roleClaims.length) {
            selectionClaim.value = convertClaimToSelect(
                props.user?.personRole?.roleClaims || []
            );
        } else {
            if (props.user?.role && props.user?.role.roleClaims.length) {
                selectionClaim.value = convertClaimToSelect(
                    props.user?.role?.roleClaims || []
                );
            }
        }
    }
);

onMounted(() => {});
</script>

<style></style>
