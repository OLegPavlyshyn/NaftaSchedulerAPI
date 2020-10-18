function addUser(user) {
  var collection = getContext().getCollection();
  var collectionLink = collection.getSelfLink();

  var isAccepted = collection.queryDocuments(
    collection.getSelfLink(),
    `SELECT * FROM c  WHERE c.Name = '${user.Name}'`,
    function (err, feed, options) {
      if (err) throw err;

      if (!feed || !feed.length) {
        collection.createDocument(collectionLink, user);
        getContext()
          .getResponse()
          .setBody(`User ${user.Name} added succesfuly`);
      } else {
        throw new Error(`User with name ${user.Name} already exists`);
      }
    }
  );

  if (!isAccepted) throw new Error("The query was not accepted by the server.");
}
