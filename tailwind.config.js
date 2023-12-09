const { addDynamicIconSelectors } = require('@iconify/tailwind');

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  corePlugins: { preflight: false },
  theme: {
    extend: {
      fontSize: {
        '2xs': ['0.625rem', '0.875rem'],
      },
      boxShadow: {
        focus: '0 0 0 0.2rem',
      },
    },
    colors: {
      transparent: 'transparent',
      white: '#ffffff',
      black: '#000000',
      'surface-a': 'var(--surface-a)',
      'surface-b': 'var(--surface-b)',
      'surface-c': 'var(--surface-c)',
      'surface-d': 'var(--surface-d)',
      'surface-e': 'var(--surface-e)',
      'surface-f': 'var(--surface-f)',
      primary: 'var(--primary-color)',
      danger: 'var(--danger-color)',
      'surface-0': 'var(--surface-0)',
      'surface-50': 'var(--surface-50)',
      'surface-100': 'var(--surface-100)',
      'surface-200': 'var(--surface-200)',
      'surface-300': 'var(--surface-300)',
      'surface-400': 'var(--surface-400)',
      'surface-500': 'var(--surface-500)',
      'surface-600': 'var(--surface-600)',
      'surface-700': 'var(--surface-700)',
      'surface-800': 'var(--surface-800)',
      'surface-900': 'var(--surface-900)',
      'gray-50': 'var(--gray-50)',
      'gray-100': 'var(--gray-100)',
      'gray-200': 'var(--gray-200)',
      'gray-300': 'var(--gray-300)',
      'gray-400': 'var(--gray-400)',
      'gray-500': 'var(--gray-500)',
      'gray-600': 'var(--gray-600)',
      'gray-700': 'var(--gray-700)',
      'gray-800': 'var(--gray-800)',
      'gray-900': 'var(--gray-900)',
      'blue-50': 'var(--blue-50)',
      'blue-100': 'var(--blue-100)',
      'blue-200': 'var(--blue-200)',
      'blue-300': 'var(--blue-300)',
      'blue-400': 'var(--blue-400)',
      'blue-500': 'var(--blue-500)',
      'blue-600': 'var(--blue-600)',
      'blue-700': 'var(--blue-700)',
      'blue-800': 'var(--blue-800)',
      'blue-900': 'var(--blue-900)',
      'green-50': 'var(--green-50)',
      'green-100': 'var(--green-100)',
      'green-200': 'var(--green-200)',
      'green-300': 'var(--green-300)',
      'green-400': 'var(--green-400)',
      'green-500': 'var(--green-500)',
      'green-600': 'var(--green-600)',
      'green-700': 'var(--green-700)',
      'green-800': 'var(--green-800)',
      'green-900': 'var(--green-900)',
      'yellow-50': 'var(--yellow-50)',
      'yellow-100': 'var(--yellow-100)',
      'yellow-200': 'var(--yellow-200)',
      'yellow-300': 'var(--yellow-300)',
      'yellow-400': 'var(--yellow-400)',
      'yellow-500': 'var(--yellow-500)',
      'yellow-600': 'var(--yellow-600)',
      'yellow-700': 'var(--yellow-700)',
      'yellow-800': 'var(--yellow-800)',
      'yellow-900': 'var(--yellow-900)',
      'cyan-50': 'var(--cyan-50)',
      'cyan-100': 'var(--cyan-100)',
      'cyan-200': 'var(--cyan-200)',
      'cyan-300': 'var(--cyan-300)',
      'cyan-400': 'var(--cyan-400)',
      'cyan-500': 'var(--cyan-500)',
      'cyan-600': 'var(--cyan-600)',
      'cyan-700': 'var(--cyan-700)',
      'cyan-800': 'var(--cyan-800)',
      'cyan-900': 'var(--cyan-900)',
      'pink-50': 'var(--pink-50)',
      'pink-100': 'var(--pink-100)',
      'pink-200': 'var(--pink-200)',
      'pink-300': 'var(--pink-300)',
      'pink-400': 'var(--pink-400)',
      'pink-500': 'var(--pink-500)',
      'pink-600': 'var(--pink-600)',
      'pink-700': 'var(--pink-700)',
      'pink-800': 'var(--pink-800)',
      'pink-900': 'var(--pink-900)',
      'indigo-50': 'var(--indigo-50)',
      'indigo-100': 'var(--indigo-100)',
      'indigo-200': 'var(--indigo-200)',
      'indigo-300': 'var(--indigo-300)',
      'indigo-400': 'var(--indigo-400)',
      'indigo-500': 'var(--indigo-500)',
      'indigo-600': 'var(--indigo-600)',
      'indigo-700': 'var(--indigo-700)',
      'indigo-800': 'var(--indigo-800)',
      'indigo-900': 'var(--indigo-900)',
      'teal-50': 'var(--teal-50)',
      'teal-100': 'var(--teal-100)',
      'teal-200': 'var(--teal-200)',
      'teal-300': 'var(--teal-300)',
      'teal-400': 'var(--teal-400)',
      'teal-500': 'var(--teal-500)',
      'teal-600': 'var(--teal-600)',
      'teal-700': 'var(--teal-700)',
      'teal-800': 'var(--teal-800)',
      'teal-900': 'var(--teal-900)',
      'orange-50': 'var(--orange-50)',
      'orange-100': 'var(--orange-100)',
      'orange-200': 'var(--orange-200)',
      'orange-300': 'var(--orange-300)',
      'orange-400': 'var(--orange-400)',
      'orange-500': 'var(--orange-500)',
      'orange-600': 'var(--orange-600)',
      'orange-700': 'var(--orange-700)',
      'orange-800': 'var(--orange-800)',
      'orange-900': 'var(--orange-900)',
      'bluegray-50': 'var(--bluegray-50)',
      'bluegray-100': 'var(--bluegray-100)',
      'bluegray-200': 'var(--bluegray-200)',
      'bluegray-300': 'var(--bluegray-300)',
      'bluegray-400': 'var(--bluegray-400)',
      'bluegray-500': 'var(--bluegray-500)',
      'bluegray-600': 'var(--bluegray-600)',
      'bluegray-700': 'var(--bluegray-700)',
      'bluegray-800': 'var(--bluegray-800)',
      'bluegray-900': 'var(--bluegray-900)',
      'purple-50': 'var(--purple-50)',
      'purple-100': 'var(--purple-100)',
      'purple-200': 'var(--purple-200)',
      'purple-300': 'var(--purple-300)',
      'purple-400': 'var(--purple-400)',
      'purple-500': 'var(--purple-500)',
      'purple-600': 'var(--purple-600)',
      'purple-700': 'var(--purple-700)',
      'purple-800': 'var(--purple-800)',
      'purple-900': 'var(--purple-900)',
      'red-50': 'var(--red-50)',
      'red-100': 'var(--red-100)',
      'red-200': 'var(--red-200)',
      'red-300': 'var(--red-300)',
      'red-400': 'var(--red-400)',
      'red-500': 'var(--red-500)',
      'red-600': 'var(--red-600)',
      'red-700': 'var(--red-700)',
      'red-800': 'var(--red-800)',
      'red-900': 'var(--red-900)',
      'primary-50': 'var(--primary-50)',
      'primary-100': 'var(--primary-100)',
      'primary-200': 'var(--primary-200)',
      'primary-300': 'var(--primary-300)',
      'primary-400': 'var(--primary-400)',
      'primary-500': 'var(--primary-500)',
      'primary-600': 'var(--primary-600)',
      'primary-700': 'var(--primary-700)',
      'primary-800': 'var(--primary-800)',
      'primary-900': 'var(--primary-900)',
    },
  },
  plugins: [
    addDynamicIconSelectors({
      prefix: 'i',
      overrideOnly: true,
    }),
  ],
};
