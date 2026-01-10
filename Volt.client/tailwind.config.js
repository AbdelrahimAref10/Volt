/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary/Button Colors - Modern Teal/Cyan - Use for primary buttons, links, active states, focus rings
        // Usage: bg-primary-500, text-primary-600, border-primary-300, hover:bg-primary-600
        primary: {
          50: '#f0fdfa',   // Very light teal
          100: '#ccfbf1',  // Light teal
          200: '#99f6e4',  // Subtle teal
          300: '#5eead4',  // Borders, dividers
          400: '#2dd4bf',  // Hover states
          500: '#14b8a6',  // Primary button color (main brand)
          600: '#0d9488',  // Primary button hover
          700: '#0f766e',  // Primary button active
          800: '#115e59',  // Darker variants
          900: '#134e4a',  // Darkest variants
          950: '#042f2e',
        },
        
        // Secondary/Neutral Colors - Use for text, backgrounds, borders, cards
        // Usage: bg-neutral-50, text-neutral-900, border-neutral-200
        neutral: {
          50: '#fafafa',   // Lightest backgrounds
          100: '#f5f5f5',  // Light backgrounds
          200: '#e5e5e5',  // Borders, dividers
          300: '#d4d4d4',  // Disabled states
          400: '#a3a3a3',  // Placeholder text
          500: '#737373',  // Secondary text
          600: '#525252',  // Body text
          700: '#404040',  // Headings
          800: '#262626',  // Dark text
          900: '#171717',  // Darkest text
          950: '#0a0a0a',
        },
        
        // Input Colors - Use for form inputs, text fields, selects
        // Usage: bg-input-50, border-input-300, focus:border-input-500
        input: {
          50: '#f9fafb',   // Input background
          100: '#f3f4f6',  // Input hover background
          200: '#e5e7eb',  // Input border
          300: '#d1d5db',  // Input border hover
          400: '#9ca3af',  // Input placeholder
          500: '#6b7280',  // Input text
          600: '#4b5563',  // Input text focus
        },
        
        // Success Colors - Use for success messages, positive actions, checkmarks
        // Usage: bg-success-50, text-success-600, border-success-200
        success: {
          50: '#f0fdf4',  // Success background
          100: '#dcfce7',  // Success light background
          200: '#bbf7d0',  // Success border
          300: '#86efac',  // Success icon
          400: '#4ade80',  // Success hover
          500: '#22c55e',  // Success main
          600: '#16a34a',  // Success dark
          700: '#15803d',
        },
        
        // Error/Danger Colors - Use for error messages, delete buttons, warnings
        // Usage: bg-error-50, text-error-600, border-error-200
        error: {
          50: '#fef2f2',   // Error background
          100: '#fee2e2',  // Error light background
          200: '#fecaca',  // Error border
          300: '#fca5a5',  // Error icon
          400: '#f87171',  // Error hover
          500: '#ef4444',  // Error main
          600: '#dc2626',  // Error dark
          700: '#b91c1c',
        },
        
        // Warning Colors - Use for warnings, alerts, caution states
        // Usage: bg-warning-50, text-warning-600
        warning: {
          50: '#fffbeb',   // Warning background
          100: '#fef3c7',  // Warning light background
          200: '#fde68a',  // Warning border
          300: '#fcd34d',  // Warning icon
          400: '#fbbf24',  // Warning hover
          500: '#f59e0b',  // Warning main
          600: '#d97706',  // Warning dark
          700: '#b45309',
        },
        
        // Info Colors - Use for informational messages, info badges
        // Usage: bg-info-50, text-info-600
        info: {
          50: '#eff6ff',   // Info background
          100: '#dbeafe',  // Info light background
          200: '#bfdbfe',  // Info border
          300: '#93c5fd',  // Info icon
          400: '#60a5fa',  // Info hover
          500: '#3b82f6',  // Info main
          600: '#2563eb',  // Info dark
          700: '#1d4ed8',
        },
        
        // Surface Colors - Use for cards, panels, modals, dropdowns
        // Usage: bg-surface-50, bg-surface-100
        surface: {
          50: '#ffffff',   // White surface (cards)
          100: '#f9fafb',  // Light surface
          200: '#f3f4f6',  // Subtle surface
          300: '#e5e7eb',  // Border surface
        },
        
        // Background Colors - Use for page backgrounds, layouts
        // Usage: bg-background-primary, bg-background-secondary
        background: {
          primary: '#ffffff',    // Main page background
          secondary: '#f8f9fa',  // Secondary background
          tertiary: '#f1f3f5',   // Tertiary background
        },
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-in-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'slide-in-down': 'slideInDown 0.3s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
        'pulse-slow': 'pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'shimmer': 'shimmer 2s linear infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideInRight: {
          '0%': { transform: 'translateX(100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' },
        },
        slideInDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        scaleIn: {
          '0%': { transform: 'scale(0.95)', opacity: '0' },
          '100%': { transform: 'scale(1)', opacity: '1' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        },
      },
    },
  },
  plugins: [],
}

