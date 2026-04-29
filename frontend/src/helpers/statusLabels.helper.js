import { useI18n } from 'vue-i18n';
import { getLabels, getLabelsPromotion } from "@/components/Status";
/**
 * Get status labels configuration 
 */
export const getStatusLabels = () => {
    const { t } = useI18n(); 
    return getLabels(t);
};
export const getStatusLabelsPromotion = () => {
    const { t } = useI18n();
    return getLabelsPromotion(t);
};
/**
 * Get a single status label by code
 * @param {string} statusCode - Status code (e.g., 'DXL', 'DXN')
 * @returns {object} Status label config with label and class properties
 */
export const getStatusLabel = (statusCode) => {
    const labels = getStatusLabels();
    return labels[statusCode] || { 
        label: statusCode, 
        class: 'text-gray-700 bg-gray-100',
        severity: 'warning' 
    };
};
export const getStatusLabelPromotion = (statusCode) => {
    const labels = getStatusLabelsPromotion();
    return (
        labels[statusCode] || {
            label: statusCode,
            class: 'text-gray-700 bg-gray-100',
            severity: 'warning'
        }
    );
};
