function isInsideQuietHours(hour, quietHours) {
  if (!quietHours || quietHours.start === quietHours.end) {
    return false;
  }

  const start = quietHours.start;
  const end = quietHours.end;

  if (start < end) {
    return hour >= start && hour < end;
  }

  return hour >= start || hour < end;
}

function buildDigest(notifications, options = {}) {
  const quietHours = options.quietHours || null;

  const items = notifications
    .filter((notification) => !notification.read)
    .filter((notification) => !isInsideQuietHours(notification.hour, quietHours))
    .map((notification) => ({
      id: notification.id,
      title: notification.title,
      category: notification.category
    }));

  return {
    total: items.length,
    categories: [...new Set(items.map((item) => item.category))],
    items
  };
}

module.exports = {
  buildDigest,
  isInsideQuietHours
};
