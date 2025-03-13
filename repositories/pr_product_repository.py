from db.connection_manager import ConnectionManager
from db.database_enum import DatabaseEnum


class PrProductRepository:
    @staticmethod
    async def get_products(article: str = None, brand: str = None):
        base_query = """
        SELECT 
            p.ID AS Product_ID,
            p.Article,
            p.Brand,
            p.Vendor_Code,
            img.Link AS Image_Link,
            a.Name AS Attribute_Name,
            pa.Value AS Attribute_Value,
            vc.Name AS Vendor_Category_Name,
            oem.Code AS OEM_Code,
            oem.Mark AS OEM_Mark,
            v.Model AS Vehicle_Model
        FROM pr_PRODUCT p
        LEFT JOIN pr_IMAGE img ON p.ID = img.Product_ID
        LEFT JOIN pr_PRODUCT_ATTRIBUTE pa ON p.ID = pa.Product_ID
        LEFT JOIN pr_ATTRIBUTE a ON pa.Attribute_ID = a.ID
        LEFT JOIN pr_PRODUCT_VENDOR_CATEGORY pvc ON p.ID = pvc.Product_ID
        LEFT JOIN pr_VENDOR_CATEGORY vc ON pvc.Vendor_Category_ID = vc.ID
        LEFT JOIN pr_OEM_ACCORD oa ON p.ID = oa.Product_ID
        LEFT JOIN pr_OEM oem ON oa.OEM_ID = oem.ID
        LEFT JOIN pr_VEHICLE_ACCORD va ON p.ID = va.Product_ID
        LEFT JOIN pr_VEHICLE v ON va.Vehicle_ID = v.ID
        WHERE 1=1
        """

        conditions = []
        params = {}
        if article:
            conditions.append("LOWER(p.Article) = LOWER(:article)")
            params["article"] = article
        if brand:
            conditions.append("LOWER(p.Brand) = LOWER(:brand)")
            params["brand"] = brand

        final_query = base_query + (" AND " + " AND ".join(conditions) if conditions else "")

        result = await ConnectionManager.execute_sql(
            database=DatabaseEnum.MNK,
            sql_query=final_query,
            parameters=params
        )

        columns = [
            "Product_ID", "Article", "Brand", "Vendor_Code",
            "Image_Link", "Attribute_Name", "Attribute_Value",
            "Vendor_Category_Name", "OEM_Code", "OEM_Mark", "Vehicle_Model"
        ]

        return [dict(zip(columns, row)) for row in result]
