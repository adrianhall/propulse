/**
 * Account Area JavaScript
 * Provides enhanced functionality for account-related forms and interactions
 */

$(document).ready(function() {
    initializeAccountForms();
    initializeAlerts();
    initializeValidation();
});

/**
 * Initialize account form enhancements
 */
function initializeAccountForms() {
    // Add loading state to form submissions
    $('form').on('submit', function() {
        const submitButton = $(this).find('button[type="submit"]');
        if (submitButton.length) {
            submitButton.addClass('loading').prop('disabled', true);
            
            // Add a timeout to prevent permanent loading state
            setTimeout(function() {
                submitButton.removeClass('loading').prop('disabled', false);
            }, 10000);
        }
    });

    // Auto-focus first input field
    const firstInput = $('.account-form-inputs input:first');
    if (firstInput.length && !firstInput.val()) {
        firstInput.focus();
    }

    // Enhanced input interactions
    $('.form-control').on('focus', function() {
        $(this).closest('.form-group').addClass('focused');
    }).on('blur', function() {
        $(this).closest('.form-group').removeClass('focused');
    });

    // Real-time email validation
    $('input[type="email"]').on('blur', function() {
        validateEmailField($(this));
    });
}

/**
 * Initialize alert functionality
 */
function initializeAlerts() {
    // Auto-dismiss success alerts after 5 seconds
    $('.alert-success').each(function() {
        const alert = $(this);
        setTimeout(function() {
            alert.fadeOut('slow');
        }, 5000);
    });

    // Add close button to alerts
    $('.alert').each(function() {
        if (!$(this).find('.btn-close').length) {
            $(this).append('<button type="button" class="btn-close float-end" aria-label="Close"></button>');
        }
    });

    // Handle alert close button clicks
    $(document).on('click', '.alert .btn-close', function() {
        $(this).closest('.alert').fadeOut('fast');
    });
}

/**
 * Initialize client-side validation enhancements
 */
function initializeValidation() {
    // Clear validation errors when user starts typing
    $('.form-control').on('input', function() {
        const field = $(this);
        const validationSpan = field.siblings('.text-danger, .field-validation-error');
        
        if (validationSpan.length && validationSpan.text().trim()) {
            validationSpan.fadeOut('fast');
        }
        
        field.removeClass('is-invalid');
    });

    // Enhanced validation display
    $('span[data-valmsg-for]').each(function() {
        const span = $(this);
        if (span.text().trim()) {
            const fieldName = span.attr('data-valmsg-for');
            const field = $(`input[name="${fieldName}"], select[name="${fieldName}"], textarea[name="${fieldName}"]`);
            field.addClass('is-invalid');
        }
    });
}

/**
 * Validate email field format
 * @param {jQuery} emailField - The email input field
 */
function validateEmailField(emailField) {
    const email = emailField.val().trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    
    if (email && !emailRegex.test(email)) {
        showFieldError(emailField, 'Please enter a valid email address.');
    } else {
        clearFieldError(emailField);
    }
}

/**
 * Show field validation error
 * @param {jQuery} field - The input field
 * @param {string} message - Error message to display
 */
function showFieldError(field, message) {
    clearFieldError(field);
    
    field.addClass('is-invalid');
    const errorSpan = $(`<span class="text-danger field-validation-error">${message}</span>`);
    field.after(errorSpan);
    errorSpan.hide().fadeIn('fast');
}

/**
 * Clear field validation error
 * @param {jQuery} field - The input field
 */
function clearFieldError(field) {
    field.removeClass('is-invalid');
    field.siblings('.text-danger, .field-validation-error').remove();
}

/**
 * Show notification message
 * @param {string} message - Message to display
 * @param {string} type - Type of notification (success, error, info, warning)
 */
function showNotification(message, type = 'info') {
    const alertClass = type === 'error' ? 'alert-danger' : `alert-${type}`;
    const iconClass = getAlertIcon(type);
    
    const alert = $(`
        <div class="alert ${alertClass} d-flex align-items-center fade show" role="alert">
            <i class="bi ${iconClass} me-2"></i>
            <div>${message}</div>
            <button type="button" class="btn-close ms-auto" aria-label="Close"></button>
        </div>
    `);
    
    // Insert at the top of the form body
    $('.account-form-body').prepend(alert);
    
    // Auto-dismiss after 5 seconds for success messages
    if (type === 'success') {
        setTimeout(() => alert.fadeOut('slow'), 5000);
    }
}

/**
 * Get appropriate icon for alert type
 * @param {string} type - Alert type
 * @returns {string} Bootstrap icon class
 */
function getAlertIcon(type) {
    switch (type) {
        case 'success':
            return 'bi-check-circle-fill';
        case 'error':
            return 'bi-exclamation-triangle-fill';
        case 'warning':
            return 'bi-exclamation-circle-fill';
        case 'info':
        default:
            return 'bi-info-circle-fill';
    }
}

/**
 * Utility function to copy text to clipboard
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} Success status
 */
async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        showNotification('Copied to clipboard!', 'success');
        return true;
    } catch (err) {
        console.error('Failed to copy text: ', err);
        showNotification('Failed to copy to clipboard', 'error');
        return false;
    }
}

/**
 * Handle keyboard navigation improvements
 */
$(document).on('keydown', function(e) {
    // Enter key on buttons
    if (e.key === 'Enter' && $(e.target).is('button:not([type="submit"])')) {
        e.target.click();
    }
    
    // Escape key to dismiss alerts
    if (e.key === 'Escape') {
        $('.alert').fadeOut('fast');
    }
});

// Export functions for potential external use
window.AccountJS = {
    showNotification,
    copyToClipboard,
    validateEmailField,
    showFieldError,
    clearFieldError
};
