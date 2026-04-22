import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = 'http://localhost:32768';
const TOKEN = __ENV.TOKEN;

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m',  target: 15 }, 
    { duration: '30s', target: 0  },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000'], 
    http_req_failed:   ['rate<0.05'], 
  },
};

export default function () {
  const randomLat = 31.85 + (Math.random() * 0.1);
  const randomLng = 35.21 + (Math.random() * 0.1);

  const headers = {
    'Authorization': `Bearer ${TOKEN}`,
    'Content-Type': 'application/json',
  };

  const body = JSON.stringify({
    latitude: randomLat,
    longitude: randomLng,
    description: `Automated Load Test Report at ${new Date().toISOString()}`,
    categoryId: 1, 
    city: 'Ramallah',
    areaName: 'Al-Masyoun',
  });

  const r = http.post(`${BASE}/api/v1/Reports/submit`, body, { headers });
  
  check(r, { 
    'report submitted (200/201)': res => res.status === 200 || res.status === 201 
  });

  sleep(1);
}