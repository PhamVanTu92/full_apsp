<template>
    <div>
        <DataTable :value="props.data">
            <Column header="#" class="w-1rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column header=""></Column>
            <Column header=""></Column>
            <Column header=""></Column>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";

interface Props {
    data: Array<any>;
}

const props = withDefaults(defineProps<Props>(), {
    data: () => [{}, {}, {}],
});

onMounted(function () {});
</script>

<style scoped></style>
