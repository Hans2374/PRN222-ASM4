/**
 * Authentication helper functions
 */

// Function to handle logout
function logout() {
    // Navigate to the logout endpoint
    window.location.href = '/Account/Logout';
}

// Function to check authentication status from cookies
function isAuthenticated() {
    // This is a simplified check - a real implementation would be more robust
    return document.cookie.indexOf('auth=') !== -1;
}

// Function to update UI based on authentication status
function updateAuthUI() {
    const authenticated = isAuthenticated();

    // Find auth-related elements
    const loginButtons = document.querySelectorAll('.login-button');
    const logoutButtons = document.querySelectorAll('.logout-button');
    const authOnlyElements = document.querySelectorAll('.auth-only');
    const guestOnlyElements = document.querySelectorAll('.guest-only');

    // Update visibility based on authentication status
    if (authenticated) {
        loginButtons.forEach(btn => btn.style.display = 'none');
        logoutButtons.forEach(btn => btn.style.display = 'inline-block');
        authOnlyElements.forEach(el => el.style.display = 'block');
        guestOnlyElements.forEach(el => el.style.display = 'none');
    } else {
        loginButtons.forEach(btn => btn.style.display = 'inline-block');
        logoutButtons.forEach(btn => btn.style.display = 'none');
        authOnlyElements.forEach(el => el.style.display = 'none');
        guestOnlyElements.forEach(el => el.style.display = 'block');
    }
}

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Attach logout handler to logout buttons
    document.querySelectorAll('.logout-button').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            logout();
        });
    });

    // Update UI based on current auth status
    updateAuthUI();
});