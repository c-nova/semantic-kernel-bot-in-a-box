param resourceLocation string
param prefix string
param tags object = {}

var uniqueSuffix = substring(uniqueString(subscription().id, resourceGroup().id), 1, 3)
var bingSearchAccountName = '${prefix}-bing-${uniqueSuffix}'

resource bingSearchAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: bingSearchAccountName
  location: resourceLocation
  tags: tags
  sku: {
    name: 'S1'
  }
  kind: 'Bing.Search.v7'
  properties: {
    customSubDomainName: bingSearchAccountName
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

output bingSearchAccountID string = bingSearchAccount.id
