import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 20,
  duration: '1m',
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1500'],
  },
};

const token = __ENV.TOKEN;

export default function () {
  const res = http.get(
    'http://localhost:5034/api/v1/Incidents/paged?pageNumber=1&pageSize=10&lang=en',
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  if (res.status !== 200) {
    console.log(`status=${res.status} body=${res.body}`);
  }

  check(res, {
    'status is 200': (r) => r.status === 200,
    'success true': (r) => {
      try {
        return r.json().success === true;
      } catch {
        return false;
      }
    },
  });

  sleep(1);
}