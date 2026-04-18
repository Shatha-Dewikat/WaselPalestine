import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE = 'http://localhost:32768';
const TOKEN = __ENV.TOKEN;

export const options = {
  stages: [
    { duration: '30s', target: 20 }, 
   { duration: '1m',  target: 25 }, 
    { duration: '30s', target: 0  }, 
  ],
  thresholds: {
    http_req_duration: ['p(95)<800'], 
    http_req_failed:   ['rate<0.05'], 
  },
};

export default function () {
  const headers = { 
    'Authorization': `Bearer ${TOKEN}`,
    'Content-Type': 'application/json'
  };

  const r1 = http.get(`${BASE}/api/v1/Incidents?page=1&pageSize=1`, { headers });
  if (r1.status !== 200) {
      console.log(`Error on Incidents: Status ${r1.status}, Body: ${r1.body}`);
  }
  const resIncidents = http.get(`${BASE}/api/v1/Incidents?page=1&pageSize=10`, { headers });
  check(resIncidents, { 'GET Incidents 200': r => r.status === 200 });

  const resAudits = http.get(`${BASE}/api/admin/auditlogs?page=1&pageSize=10`, { headers });
  check(resAudits, { 'GET AuditLogs 200': r => r.status === 200 });

  const resCheckpoints = http.get(`${BASE}/api/v1/Checkpoints?page=1&pageSize=10`, { headers });
  check(resCheckpoints, { 'GET Checkpoints 200': r => r.status === 200 });


  const reportPayload = JSON.stringify({
    latitude: 31.9,
    longitude: 35.2,
    description: "Automated Load Test Report",
    categoryId: 1,
    city: "Ramallah",
    areaName: "Al-Masyoun"
  });

  const resPostReport = http.post(`${BASE}/api/v1/Reports/submit`, reportPayload, { headers });
  check(resPostReport, { 'POST Report 200/201': r => r.status === 200 || r.status === 201 });

  
  const resUsers = http.get(`${BASE}/api/admin/users?page=1&pageSize=5`, { headers });
  check(resUsers, { 'GET Users List 200': r => r.status === 200 });

  sleep(1);
}