/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      colors: {
        // Deep steel-navy — sidebar / chrome
        steel: {
          950: '#0B121A',
          900: '#101A24',
          800: '#16222F',
          700: '#20313F',
          600: '#324357',
          500: '#4A5D71',
          400: '#6B7D8F',
          300: '#91A4B8',
          200: '#B9C8D9',
          100: '#DCE6F1'
        },
        // Neutral working surface — main content area
        panel: {
          50: '#F5F7F9',
          100: '#ECEFF2',
          200: '#DDE3E8',
          300: '#C7CFD7'
        },
        // Safety-amber — primary action / warning accent (factory hazard signage)
        amber: {
          400: '#F6A93B',
          500: '#F0961A',
          600: '#D67F0C'
        },
        // Line-status teal — success / running state
        line: {
          400: '#3FC7B8',
          500: '#1FA695',
          600: '#178578'
        },
        // Alert red — rejected/stopped state
        stop: {
          400: '#EF6A5C',
          500: '#E14A3A'
        }
      },
      fontFamily: {
        display: ['"Space Grotesk"', 'sans-serif'],
        body: ['"IBM Plex Sans"', 'sans-serif'],
        mono: ['"IBM Plex Mono"', 'monospace']
      }
    }
  },
  plugins: []
}
