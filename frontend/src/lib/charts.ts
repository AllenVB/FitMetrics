// Chart.js bileşenlerini bir kez kaydeder (tree-shaking için gerekli).
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LineElement,
  LinearScale,
  PointElement,
  Tooltip,
} from 'chart.js';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Tooltip,
  Legend,
  Filler,
);

// Koyu tema için global Chart.js varsayılanları
ChartJS.defaults.color = '#c2c6d6';
ChartJS.defaults.borderColor = 'rgba(255, 255, 255, 0.06)';
ChartJS.defaults.font.family = 'Inter, system-ui, sans-serif';

// Tema renk paleti (grafiklerde tutarlı kullanım için)
export const chartColors = {
  primary: '#adc6ff',
  primaryFill: 'rgba(173, 198, 255, 0.14)',
  tertiary: '#2fe6a0',
  tertiaryFill: 'rgba(47, 230, 160, 0.14)',
  secondary: '#c2a4ff',
  secondaryFill: 'rgba(194, 164, 255, 0.14)',
  error: '#ffb4ab',
  errorFill: 'rgba(255, 180, 171, 0.12)',
  blue: '#4d8eff',
  outline: '#8c909f',
};
