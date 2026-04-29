<template>
    <div class="custom-rating">
        <span v-for="star in stars" :key="star" class="star"
            :class="{ 'filled': star <= modelValue, 'hovered': star <= hoverValue }" @click="setRating(star)"
            @mouseenter="hoverValue = star" @mouseleave="hoverValue = 0">
            <i :class="star <= modelValue ? 'fas fa-star' : 'far fa-star'"></i>
        </span>
        <span v-if="cancel" class="cancel" @click="setRating(0)">
            <i class="fas fa-times"></i>
        </span>
    </div>
</template>

<script setup>
import { ref } from 'vue';
import '../style/rating.css'

const props = defineProps({
    modelValue: {
        type: Number,
        default: 0
    },
    stars: {
        type: Number,
        default: 5
    },
    cancel: {
        type: Boolean,
        default: false
    },
    disabled: {
        type: Boolean,
        default: false
    }
});

const emit = defineEmits(['update:modelValue']);

const hoverValue = ref(0);

const setRating = (value) => {
    if (!props.disabled) {
        emit('update:modelValue', value);
    }
};
</script> 