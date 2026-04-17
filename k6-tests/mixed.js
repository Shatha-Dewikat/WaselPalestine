import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    readers: {
      executor: 'constant-vus',
      vus: 5,
      duration: '1m',
      exec: 'readFlow',
    },
    writers: {
      executor: 'constant-vus',
      vus: 3,
      duration: '1m',
      exec: 'writeFlow',
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.25'],
  },
};

const token = __ENV.TOKEN;

export function readFlow() {
  const res = http.get(
    'http://localhost:5034/api/v1/Incidents/paged?pageNumber=1&pageSize=10&lang=en',
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );

  check(res, {
    'read status 200': (r) => r.status === 200,
  });

  sleep(1);
}

export function writeFlow() {
  const payload = JSON.stringify({
    latitude: 31.501,
    longitude: 34.466,
    description: `k6 mixed write ${__VU}-${__ITER}`,
    categoryId: 1,
    userId: 'f3db18bc-01cd-43ec-9cf3-34cc01c7bc80',
    city: 'Nablus',
    areaName: 'Downtown',
  });

  const res = http.post(
    'http://localhost:5034/api/v1/Reports/submit',
    payload,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    }
  );

  check(res, {
    'write status ok': (r) => r.status === 200 || r.status === 201,
  });

  sleep(1);
}