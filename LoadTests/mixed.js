import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = 'http://localhost:32768';
const TOKEN = __ENV.TOKEN;

export const options = {
  stages: [
    { duration: '1m', target: 30 },
    { duration: '2m', target: 30 },
    { duration: '30s', target: 0 },
  ],
};

export default function () {
  const headers = { 'Authorization': `Bearer ${TOKEN}`, 'Content-Type': 'application/json' };
  
  if (Math.random() < 0.7) {
    // READ (70%)
    http.get(`${BASE}/api/v1/Incidents?page=1&pageSize=1`, { headers });
  } else {
    // WRITE (30%)
    const body = JSON.stringify({
      latitude: 31.8, longitude: 35.2, description: "Mixed Load Test", categoryId: 1, city: "Nablus", areaName: "Rafidia"
    });
    http.post(`${BASE}/api/v1/Reports/submit`, body, { headers });
  }
  sleep(1);
}