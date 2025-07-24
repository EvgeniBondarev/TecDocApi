from typing import List, Dict


class PrProductService:
    def __init__(self, repository):
        self.repository = repository

    async def get_products(self, article: str = None, brand: str = None) -> List[Dict]:
        raw_data = await self.repository.get_products(article, brand)
        return self._transform_data(raw_data)

    async def get_links_by_td_id(self, td_id: int) -> List[Dict]:
        return await self.repository.get_links_by_td_id(td_id)

    async def get_links_by_jc_id(self, jc_id: int) -> List[Dict]:
        return await self.repository.get_links_by_jc_id(jc_id)

    def _transform_data(self, raw_data: List[Dict]) -> List[Dict]:
        products = {}
        for row in raw_data:
            product_id = row["Product_ID"]
            if product_id not in products:
                products[product_id] = {
                    "article": row["Article"],
                    "brand": row["Brand"],
                    "Vendor_Code": row["Vendor_Code"],
                    "attributes": {},
                    "Vendor_Category_Name": row["Vendor_Category_Name"],
                    "OEM_Code": row["OEM_Code"],
                    "OEM_Mark": row["OEM_Mark"],
                    "models": set(),
                    "images": set(),
                }

            if row["Attribute_Name"] and row["Attribute_Value"]:
                products[product_id]["attributes"][row["Attribute_Name"]] = row["Attribute_Value"]

            if row["Vehicle_Model"]:
                products[product_id]["models"].add(row["Vehicle_Model"])

            if row["Image_Link"]:
                products[product_id]["images"].add(row["Image_Link"])

        return [
            {
                **product,
                "attributes": [{"name": k, "value": v} for k, v in product["attributes"].items()],
                "models": list(product["models"]),
                "images": list(product["images"])
            }
            for product in products.values()
        ]

